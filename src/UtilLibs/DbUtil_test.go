package UtilLibs



import (
	"database/sql"
	"fmt"
	_ "github.com/go-sql-driver/mysql"
	"math/rand"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"strconv"
	"testing"
	"time"
)
const filename = "SampleFile.txt"

var dbCfg DataModels.DBConfiguration
func init(){
	dbCfg = DataModels.DBConfiguration{"mysql", "35.197.120.109", 3306, "root", "Vanamali12#", "estateAIDbTest","TestCounty"}

}

func TestDBActivities(t *testing.T) {
	db, err := ConnectToDB(dbCfg)
	if err != nil {
		// Test Failed Due to connectivity to DB
		fmt.Printf("Error :%v", err)
		return
	}
	defer db.Close()

	fmt.Printf("Ping Ok,Printing Latest JOB ID\n")
	// Get Max Results
	executeAndPrintRecords(LastJobIDQuery, db)
	executeAndPrintRecords(AllJobsQuery, db)
	executeAndPrintRecords(CurrentJobCount, db)
}

func TestDBInsert(t *testing.T) {
	tmpInsertValuesStr:=getPropValuesStr(2)
	fmt.Printf("Insert Values :%v",tmpInsertValuesStr)
	// Insert some dummy prop records
	err := ExecuteDbStatInsertQueryNow(dbCfg, tmpInsertValuesStr)
	if err != nil {
		fmt.Errorf("Error :%v", err)
	}

}


func TestDbCreateTbl(t *testing.T) {
	err:= executeTableCreation(dbCfg)
	if err!= nil {
		fmt.Printf("Error creating table ... %v",err)
	}
}

func TestCheckTbl(t *testing.T) {
	tblExsists:= IsTableExists(dbCfg,dbCfg.TableName)
	if tblExsists {
		fmt.Printf("Table exists ")
	} else {
		fmt.Printf("Table Doesn't exisit ")
	}
}

func TestDbDropTbl(t *testing.T) {
	err:= executeDropTableQuery(dbCfg)
	if err!= nil {
		fmt.Printf("Error Dropping table ... %v",err)
	}
}

func TestDbCreateView(t *testing.T) {
	err:= executeViewCreation(dbCfg)
	if err!= nil {
		fmt.Printf("Error creating View ... %v",err)
	}
}

func TestDbDropView(t *testing.T) {
	err:= executeDropView(dbCfg)
	if err!= nil {
		fmt.Printf("Error Dropping View ... %v",err)
	}
}

func TestDropAndCreateTableAndViewCreation(t *testing.T){
	executeDropTableQuery(dbCfg)
	executeDropView(dbCfg)

	// now create table and view
	result := CreateTableAndView(dbCfg)
	if result {
		fmt.Printf("Table creation is sucessful .. ")
	} else {
		fmt.Printf("problem creating tables and view ")
	}

}

func TestCheckTableAndViewCreation(t *testing.T){
	// now create table and view
	result := CreateTableAndView(dbCfg)
	if result {
		fmt.Printf("Table exists or creation is sucessful .. ")
	} else {
		fmt.Printf("problem creating tables and view ")
	}

}


func getPropValuesStr(count int) string {
	// Let us start Prep to write Data to DB
	jobId := time.Now().Unix()
	resultedValuesStr := ""
	isFirstElement := true
	totalRecordsToWrote := 0
	for i := 0; i < count; i++ {
		prop := RandomPropGen("TestSample", jobId)
		if isFirstElement {
			resultedValuesStr = fmt.Sprintf("%v", prop.GetInsertRecordForSQL())
			isFirstElement = false
		} else {
			resultedValuesStr = fmt.Sprintf("%v,%v", resultedValuesStr, prop.GetInsertRecordForSQL())
		}
		totalRecordsToWrote++
	}

	return resultedValuesStr
}

func executeAndPrintRecords(query string, db *sql.DB) {
	rows, err := db.Query(query)
	if err != nil {
		fmt.Printf("Error:%v\n", err.Error())
		return
	}
	defer rows.Close()
	for rows.Next() {
		var id int
		err := rows.Scan(&id)
		if err != nil {
			fmt.Printf("Error: %\n", err)
		} else {
			fmt.Printf("Result Record :%v\n", id)
		}
	}
}



func RandomPropGen(prefixId string, jobId int64) DataModels.PropertyModel {
	currTime := time.Now()
	prop := DataModels.PropertyModel{}
	prop.CrawlJobId = jobId
	prop.DateCrawled = fmt.Sprintf("%04d%02d%02d%02d", currTime.Year(), currTime.Month(), currTime.Day(), currTime.Hour())
	prop.WalkScore = 4
	prop.ZipCode = "98075"
	prop.State = "WA"
	prop.City = "Bellevue"
	prop.Address = prefixId
	prop.Price = 235000
	prop.MLSID = strconv.Itoa(rand.Int())
	fmt.Printf("%v\n", prop)
	return prop
}
