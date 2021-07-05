package DataModels

import (
	"fmt"
	_ "github.com/go-sql-driver/mysql"
	"testing"
)

func TestCreateTableQuery(t *testing.T) {

	sqlStr := CreatetableSqlQuery("TestCountyTbl")


	fmt.Printf("Table Query:%v",sqlStr)
}
