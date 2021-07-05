package DataHaulingEngine

import (
	"encoding/csv"
	"fmt"
	"github.com/golang/glog"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"nrideas.visualstudio.com/EstateAI/src/UtilLibs"
	"os"
	"sync"
)

type lockresource struct {
	sync.Mutex
	purpose string
}

var lockObject *lockresource

func init() {
	lockObject = &lockresource{purpose: "FileWriteLock"}
}

const MAX_ToHOLD_BEFORE_WRITE_TO_FILE = 200

type DatahaulBuffer struct {
	DataStore  []DataModels.PropertyModel
	FileName   string
	ObjectGoId string
}

func CreateDataBuffStore(filename string, id string) DatahaulBuffer {

	dataBuf := DatahaulBuffer{DataStore: make([]DataModels.PropertyModel, 0), FileName: filename, ObjectGoId: id}
	return dataBuf
}

func (helper *DatahaulBuffer) AddpropertyObject(prop *DataModels.PropertyModel) bool {

	if len(helper.DataStore) < MAX_ToHOLD_BEFORE_WRITE_TO_FILE {
		helper.DataStore = append(helper.DataStore, *prop)
		if len(helper.DataStore) < MAX_ToHOLD_BEFORE_WRITE_TO_FILE {
			return true
		}
	}
	return false
}

func (helper *DatahaulBuffer) IsFull() bool {
	if len(helper.DataStore) < MAX_ToHOLD_BEFORE_WRITE_TO_FILE {
		return false
	}
	return true
}

func (helper *DatahaulBuffer) WriteToFile() error {
	lockObject.Lock()
	defer lockObject.Unlock()
	// Let us check the data buffer size before attemnping to append to the file.

	if len(helper.DataStore) == 0 {
		// Empty List to write, so skip writing.
		//glog.Infof("Data Store is Empty:%v",helper.ObjectGoId)
		return nil
	}

	fileExists := true
	// Check for File existance
	fi, err := os.Lstat(helper.FileName)
	if err != nil {
		fileExists = false
	} else if fi.Size() == 0 {
		fileExists = false
	}

	// Open File for append and start writing
	f, err := os.OpenFile(helper.FileName, os.O_APPEND|os.O_CREATE|os.O_WRONLY, 0644)
	if err != nil {
		return err
	}

	defer f.Close()
	csvWriter := csv.NewWriter(f)
	defer csvWriter.Flush()

	if !fileExists {
		// Create Header first and then Data.
		glog.Infof("Adding CSV header first ..")
		err = csvWriter.Write(DataModels.GetPropHeader())
		if err != nil {
			return err
		}
	}

	for _, prop := range helper.DataStore {
		err = csvWriter.Write(prop.GetCSVFormattedStrings())
		if err != nil {
			return err
		}
	}

	return nil
}

/*
 *    Database related operations
 */

 // returns true on sucessful creation or false on errors

func (helper *DatahaulBuffer) WriteToDB(dbConfig DataModels.DBConfiguration) error {
	lockObject.Lock()
	defer lockObject.Unlock()

	if len(helper.DataStore) == 0 {
		// Empty List to write, so skip writing.
		//glog.Infof("Data Store is Empty:%v,Skipping to write to DB",helper.ObjectGoId)
		return nil
	}

	//glog.Infof(" Writing to database engine @ %v, Database : %v, Table Name:%v", dbConfig.DBHost, dbConfig.DBName,dbConfig.TableName)
	// Let us start Prep to write Data to DB
	resultedValuesStr := ""
	isFirstElement := true
	totalRecordsToWrite := 0
	for _, prop := range helper.DataStore {
		if isFirstElement {
			resultedValuesStr = fmt.Sprintf("%v", prop.GetInsertRecordForSQL())
			isFirstElement = false
		} else {
			resultedValuesStr = fmt.Sprintf("%v,%v", resultedValuesStr, prop.GetInsertRecordForSQL())
		}
		totalRecordsToWrite++
	}

	if totalRecordsToWrite > 0 {
		go UtilLibs.ExecuteDbInsertQueryNow(dbConfig,resultedValuesStr)
	}
	return nil
}


