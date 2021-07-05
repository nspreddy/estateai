package DataModels

import (
	"reflect"
	"fmt"
)

const (
	CREATE_MYSQL_TBALEFORMATTER="CREATE TABLE %v ( %v ) ENGINE=InnoDB DEFAULT CHARSET=utf8;"
	CREATE_MSSQL_TBALEFORMATTER="CREATE TABLE %v ( %v ) ;"
)

func GetMySqlCreateTableQuery(tablename string, propData *reflect.Value) string {

	sqlCreateStr := ""
	for i := 0; i < propData.NumField(); i++ {
		if i > 0 {
			sqlCreateStr += ","
		}
		field := propData.Field(i)
		//dataelements:=field
		switch field.Kind() {
		case reflect.String:
			sqlCreateStr += fmt.Sprintf("`%v`  text", propData.Type().Field(i).Name)
		case reflect.Uint:
			sqlCreateStr += fmt.Sprintf("`%v`  int(11) DEFAULT NULL", propData.Type().Field(i).Name)
		case reflect.Int:
			sqlCreateStr += fmt.Sprintf("`%v`  int(11) DEFAULT NULL", propData.Type().Field(i).Name)
		case reflect.Int64:
			sqlCreateStr += fmt.Sprintf("`%v`  bigint(20) DEFAULT NULL", propData.Type().Field(i).Name)
		case reflect.Float32, reflect.Float64:
			sqlCreateStr += fmt.Sprintf("`%v`  int(11) DEFAULT NULL", propData.Type().Field(i).Name)
		default:
			sqlCreateStr += fmt.Sprintf("`%v`  text", propData.Type().Field(i).Name)
		}
	}

	formattedStr := fmt.Sprintf(CREATE_MYSQL_TBALEFORMATTER,tablename,sqlCreateStr)
	return formattedStr
}

func GetMSSqlCreateTableQuery(tablename string, propData *reflect.Value) string {

	sqlCreateStr := ""
	for i := 0; i < propData.NumField(); i++ {
		if i > 0 {
			sqlCreateStr += ","
		}
		field := propData.Field(i)
		//dataelements:=field
		switch field.Kind() {
		case reflect.String:
			sqlCreateStr += fmt.Sprintf("%v  text", propData.Type().Field(i).Name)
		case reflect.Uint:
			sqlCreateStr += fmt.Sprintf("%v  int DEFAULT NULL", propData.Type().Field(i).Name)
		case reflect.Int:
			sqlCreateStr += fmt.Sprintf("%v  int DEFAULT NULL", propData.Type().Field(i).Name)
		case reflect.Int64:
			sqlCreateStr += fmt.Sprintf("%v  bigint DEFAULT NULL", propData.Type().Field(i).Name)
		case reflect.Float32, reflect.Float64:
			sqlCreateStr += fmt.Sprintf("%v  int DEFAULT NULL", propData.Type().Field(i).Name)
		default:
			sqlCreateStr += fmt.Sprintf("%v  text", propData.Type().Field(i).Name)
		}
	}

	formattedStr := fmt.Sprintf(CREATE_MSSQL_TBALEFORMATTER,tablename,sqlCreateStr)
	return formattedStr
}

func GetSQLDataRecordToInsert(propElements *reflect.Value) string {
	sqlInsertRecordStr := `(`
	for i := 0; i < propElements.NumField(); i++ {
		if i > 0 {
			sqlInsertRecordStr += ","
		}
		field := propElements.Field(i)
		switch field.Kind() {
		case reflect.String:
			sqlInsertRecordStr += fmt.Sprintf(`"%v"`, field.String())
		case reflect.Uint:
			sqlInsertRecordStr += fmt.Sprintf(`%v`, field.Uint())
		case reflect.Int:
			sqlInsertRecordStr += fmt.Sprintf(`%v`, field.Int())
		case reflect.Int64:
			sqlInsertRecordStr += fmt.Sprintf(`%v`, field.Int())
		case reflect.Float32, reflect.Float64:
			sqlInsertRecordStr += fmt.Sprintf(`%v`, field.Float())
		default:
			sqlInsertRecordStr += fmt.Sprintf(`"%v"`, field.String())
		}
	}
	sqlInsertRecordStr += `)`
	return sqlInsertRecordStr
}

func GetMsSQLSQLDataRecordToInsert(propElements *reflect.Value) string {

	sqlInsertRecordStr := `(`

	for i := 0; i < propElements.NumField(); i++ {
		if i > 0 {
			sqlInsertRecordStr += ","
		}
		field := propElements.Field(i)
		switch field.Kind() {
		case reflect.String:
			sqlInsertRecordStr += fmt.Sprintf(`"%v"`, field.String())
		case reflect.Uint:
			sqlInsertRecordStr += fmt.Sprintf(`%v`, field.Uint())
		case reflect.Int:
			sqlInsertRecordStr += fmt.Sprintf(`%v`, field.Int())
		case reflect.Int64:
			sqlInsertRecordStr += fmt.Sprintf(`%v`, field.Int())
		case reflect.Float32, reflect.Float64:
			sqlInsertRecordStr += fmt.Sprintf(`%v`, field.Float())
		default:
			sqlInsertRecordStr += fmt.Sprintf(`"%v"`, field.String())
		}
	}

	sqlInsertRecordStr += `)`

	return sqlInsertRecordStr
}

func  GetDataAsSlice(propElements *reflect.Value) []string {

	dataelements := make([]string, propElements.NumField())

	for i := 0; i < propElements.NumField(); i++ {
		field := propElements.Field(i)
		//dataelements:=field
		switch field.Kind() {
		case reflect.String:
			dataelements[i] = field.String()
		case reflect.Uint:
			dataelements[i] = fmt.Sprintf("%v", field.Uint())
		case reflect.Int:
			dataelements[i] = fmt.Sprintf("%v", field.Int())
		case reflect.Int64:
			dataelements[i] = fmt.Sprintf("%v", field.Int())
		case reflect.Float32, reflect.Float64:
			dataelements[i] = fmt.Sprintf("%v", field.Float())

		default:
			dataelements[i] = "N/A"
		}
	}
	return dataelements
}

func GetHeader(propData *reflect.Value) []string {

	header := make([]string, propData.NumField())

	for i := 0; i < propData.NumField(); i++ {
		header[i] = propData.Type().Field(i).Name
	}

	return header
}