package DataHaulingEngine

import (
	"fmt"
	_ "github.com/go-sql-driver/mysql"
	"math/rand"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"os"
	"strconv"
	"testing"
	"time"
)

const filename = "SampleFile.txt"

var dbCfg DataModels.DBConfiguration
func init(){
	dbCfg = DataModels.DBConfiguration{"mysql", "35.197.120.109", 3306, "root", "Vanamali12#", "estateAIDbTest","TestCounty"}

}

func TestDataHaulWriteSingle(t *testing.T) {

	//test with 10  go routiniues in parllel..
	for i := 0; i < 10; i++ {
		prefixStr := fmt.Sprintf("PREFIX_%d", i)
		generateContentAndWriteToFile(prefixStr)
	}
}

func generateContentAndWriteToFile(prefixId string) {
	dataBuf := CreateDataBuffStore(filename, prefixId)
	for i := 0; i < 5; i++ {
		tmpprop := RandomPropGen(prefixId, 0)
		dataBuf.AddpropertyObject(&tmpprop)
	}

	err := dataBuf.WriteToFile()
	if err != nil {
		fmt.Printf("Error: %v", err)
	}
}

func TestDataBuffersAndWriteToFile(t *testing.T) {

	//test with 10  go routiniues in parllel..
	for i := 0; i < 10; i++ {
		prefixStr := fmt.Sprintf("PREFIX_%d", i)
		dataBuf := CreateDataBuffStore(filename, prefixStr)

		if i == 1 {
			for j := 0; j < 5; j++ {
				tmpprop := RandomPropGen(prefixStr, 0)
				dataBuf.AddpropertyObject(&tmpprop)
			}
		}
		err := dataBuf.WriteToFile()
		if err != nil {
			fmt.Printf("Error: %v", err)
		}
	}
}

func TestPrintPropDataList(t *testing.T) {

	prop := RandomPropGen("TestSample", 0)
	for i, elementStr := range DataModels.GetPropHeader() {
		fmt.Printf("%d:%v\n", i, elementStr)
	}

	for i, elementStr := range prop.GetCSVFormattedStrings() {
		fmt.Printf("%d:%v\n", i, elementStr)
	}
}

func TestFileExists(t *testing.T) {
	fi, err := os.Lstat(filename)
	if err != nil {
		fmt.Printf("Err:%v", err)
		return
	}

	fmt.Printf(" File Details :%v ", fi)
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
