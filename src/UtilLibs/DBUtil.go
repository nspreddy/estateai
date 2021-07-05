package UtilLibs

import (
	"database/sql"
	"fmt"
	//_ "github.com/go-sql-driver/mysql"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"github.com/golang/glog"
	"nrideas.visualstudio.com/EstateAI/src/MetricsAndStats"
	"github.com/GoogleCloudPlatform/cloudsql-proxy/proxy/dialers/mysql"
	"io/ioutil"
	"golang.org/x/net/context"
	"github.com/GoogleCloudPlatform/cloudsql-proxy/proxy/proxy"
	goauth "golang.org/x/oauth2/google"
	"net/http"
	_ "github.com/denisenkom/go-mssqldb"
)

const (
	LastJobIDQuery  = "SELECT Max(CrawlJobId)  FROM EstateCrawlData"
	AllJobsQuery    = "SELECT CrawlJobId FROM  EstateCrawlData GROUP BY CrawlJobId"
	CurrentJobCount = "SELECT COUNT(*) FROM FreshlyCrawledData"
	InsertStmt      = "INSERT INTO %v VALUES "
	DBConnectionStr = "%v:%v@tcp(%v:%v)/%v"
	DBGcpConnStr    = "%s:%s@cloudsql(%s)/%v"
)

const(
	DROPTBLQUERYFORMAT    = "DROP TABLE `%v`.`%v`;"
	CHECKTBLQUERYFORMAT   = "SHOW TABLES LIKE '%v';"
)

const(
	CREATEVIEWFORMAT       = "CREATE VIEW `Fresh%v` AS  SELECT  *  FROM %v where  CrawlJobId = (SELECT MAX(CrawlJobId) FROM  %v)"
	DROPVIEWFORMAT         = "DROP VIEW `%v`.`Fresh%v`;"
	VIEWTBLNAME   = "Fresh%v"
)

const(
	STATSTBLNAME             =  "JobStats%v"
	STATSKEYVALUEPAIRTBLNAME =  "JobStatsKVPair%v"
)

const SQLScope = "https://www.googleapis.com/auth/sqlservice.admin"

func InitDBClientAndProxy(datHaulerCfg DataModels.DataHaulingConfiguration) error{

	if !datHaulerCfg.WriteToDb {
		return nil
	}
	switch datHaulerCfg.DbConfig.Provider {
	case "mysql":
			client, err := clientFromCredentials(datHaulerCfg.DbConfig.GcpTknFile)
			if err != nil {
				return fmt.Errorf("Token Error:%v", err)
			}
			proxy.Init(client, nil, nil)
	case "mssql","sqlserver":
		    glog.Infof("Provide is Sql Server/MSSQL")
	}
	return nil
}

func clientFromCredentials( tokenfilename string) (*http.Client, error) {

	var client *http.Client
	ctx := context.Background()

	if f := tokenfilename; f != "" {
		all, err := ioutil.ReadFile(f)
		if err != nil {
			return nil, fmt.Errorf("invalid json file %q: %v", f, err)
		}

		cfg, err := goauth.JWTConfigFromJSON(all, SQLScope)
		if err != nil {
			return nil, fmt.Errorf("invalid json file %q: %v", f, err)
		}

		client = cfg.Client(ctx)
		return client, nil
	}

	return nil, fmt.Errorf("Token File not present")
}

func CreateTableAndView(dbConfig DataModels.DBConfiguration) bool {
	if !IsTableExists(dbConfig,dbConfig.TableName) {
		err:= executeTableCreation(dbConfig)
		if err!= nil {
			glog.Errorf("Error creating table ... %v",err)
			return false
		}
	}

	viewTblName := fmt.Sprintf(VIEWTBLNAME,dbConfig.TableName)
	if !IsTableExists(dbConfig,viewTblName) {
		err := executeViewCreation(dbConfig)
		if err != nil {
			glog.Errorf("Error creating view ... %v", err)
			return false
		}
	}

	return true
}

func CreateStatsTable(dbConfig DataModels.DBConfiguration) error{
	// Stats Table
	jobStatsTblName := fmt.Sprintf(STATSTBLNAME,dbConfig.TableName)
	if IsTableExists(dbConfig,jobStatsTblName) {
		glog.Infof("Table:%v Already exists",jobStatsTblName)
		return nil
	}

	// Check for Table existing before creation..
	SqlStmt := MetricsAndStats.GetSqlCreateTableQuery(jobStatsTblName)
	rows,err := executeDbQueryWithNoResults(dbConfig,SqlStmt)
	if err != nil {
		return fmt.Errorf("Error Creating Stats Table :%v Quetry:%v.. Error:%v\n",dbConfig.TableName,SqlStmt,err)
	}
	rows.Close()
	return nil
}

func CreateStatsKeyValuePairTable(dbConfig DataModels.DBConfiguration) error{
	// Stats Table
	jobStatsTblName := fmt.Sprintf(STATSKEYVALUEPAIRTBLNAME,dbConfig.TableName)
	if IsTableExists(dbConfig,jobStatsTblName) {
		glog.Infof("Table:%v Already exists",jobStatsTblName)
		return nil
	}

	// Check for Table existing before creation..
	SqlStmt := MetricsAndStats.GetSqlKVPairCreateTableQuery(jobStatsTblName)
	rows,err := executeDbQueryWithNoResults(dbConfig,SqlStmt)
	if err != nil {
		return fmt.Errorf("Error Creating Stats Table :%v Quetry:%v.. Error:%v\n",dbConfig.TableName,SqlStmt,err)
	}
	rows.Close()
	return nil
}


func GetInsertQueryPrefix(tablename string) string{
	return fmt.Sprintf(InsertStmt,tablename)
}

// Inserting property data
func ExecuteDbInsertQueryNow(dbConfig DataModels.DBConfiguration,valuesStr string) error {
	// Check for Table existing before creation..
	SqlStmt := fmt.Sprintf("%v %v", GetInsertQueryPrefix(dbConfig.TableName), valuesStr)
	rows,err := executeDbQueryWithNoResults(dbConfig,SqlStmt)
	if err != nil {
		return fmt.Errorf("Error Inserting into Table:%v Quetry:%v.. Error:%v\n",dbConfig.TableName,SqlStmt,err)
	}
	rows.Close()
	return nil
}

// Inserting Stats
func ExecuteDbStatInsertQueryNow(dbConfig DataModels.DBConfiguration,valuesStr string) error {
	// Check for Table existing before creation..
	jobStatsTblName := fmt.Sprintf(STATSTBLNAME,dbConfig.TableName)
	SqlStmt := fmt.Sprintf("%v %v", GetInsertQueryPrefix(jobStatsTblName), valuesStr)
	//glog.Infof("SQL Stats query: %v",SqlStmt)
	rows,err := executeDbQueryWithNoResults(dbConfig,SqlStmt)
	if err != nil {
		return fmt.Errorf("Error Inserting into Table:%v Quetry:%v.. Error:%v\n",dbConfig.TableName,SqlStmt,err)
	}
	rows.Close()
	return nil
}

// Inserting KV Pair Stats
func ExecuteDbKVPairStatsInsertQueryNow(dbConfig DataModels.DBConfiguration,valuesStr string) error {
	// Check for Table existing before creation..
	jobStatsTblName := fmt.Sprintf(STATSKEYVALUEPAIRTBLNAME,dbConfig.TableName)
	SqlStmt := fmt.Sprintf("%v %v", GetInsertQueryPrefix(jobStatsTblName), valuesStr)
	//glog.Infof("SQL Stats KV Pair query: %v",SqlStmt)
	rows,err := executeDbQueryWithNoResults(dbConfig,SqlStmt)
	if err != nil {
		return fmt.Errorf("Error Inserting into Table:%v Quetry:%v.. Error:%v\n",dbConfig.TableName,SqlStmt,err)
	}
	rows.Close()
	return nil
}

/*
 *    Table create and drop functions
 */
func executeTableCreation(dbConfig DataModels.DBConfiguration) error {
	// Check for Table existing before creation..
	SqlStmt := ""
	switch dbConfig.Provider {
	case "mysql":
		SqlStmt = DataModels.CreatetableMySqlQuery(dbConfig.TableName)
	case "mssql","sqlserver":
		SqlStmt = DataModels.CreatetableMsSqlQuery(dbConfig.TableName)
	}

	glog.Infof("Creating Table with SQL : %v",SqlStmt)
	rows,err := executeDbQueryWithNoResults(dbConfig,SqlStmt)
	if err != nil {
		return fmt.Errorf("Error Creating Table :%v Error:%v\n",dbConfig.TableName,err)
	}
	rows.Close()
	return nil
}




func IsTableExists(dbConfig DataModels.DBConfiguration, tablename string) bool{
	SqlStmt := fmt.Sprintf(CHECKTBLQUERYFORMAT,tablename)
	rows,err := executeDbQueryWithNoResults(dbConfig,SqlStmt)
	if err != nil {
		glog.Errorf("Error Checking for Table :%v Quetry:%v.. Error:%v\n",dbConfig.TableName,SqlStmt,err)
		return false
	}
	defer rows.Close()
	// check for rows size..
	if rows.Next() {
		// Table exists
		return true
	}
	return false
}



func executeDropTableQuery(dbConfig DataModels.DBConfiguration) error {
	SqlStmt := fmt.Sprintf(DROPTBLQUERYFORMAT,dbConfig.DBName,dbConfig.TableName)
	rows,err := executeDbQueryWithNoResults(dbConfig,SqlStmt)
	if err != nil {
		return fmt.Errorf("Error Dropping Table :%v Quetry:%v.. Error:%v\n",dbConfig.TableName,SqlStmt,err)
	}
	rows.Close()
	return nil
}


func executeViewCreation(dbConfig DataModels.DBConfiguration) error {
	// Check for Table existing before creation..
	SqlStmt := fmt.Sprintf(CREATEVIEWFORMAT,dbConfig.TableName,dbConfig.TableName,dbConfig.TableName)
	glog.Infof("Creating view with SQL : %v",SqlStmt)
	rows,err := executeDbQueryWithNoResults(dbConfig,SqlStmt)
	if err != nil {
		return fmt.Errorf("Error Creating View :%v Quetry:%v.. Error:%v\n",dbConfig.TableName,SqlStmt,err)
	}
	rows.Close()
	return nil
}

func executeDropView(dbConfig DataModels.DBConfiguration) error {
	// Check for Table existing before creation..
	SqlStmt := fmt.Sprintf(DROPVIEWFORMAT,dbConfig.DBName,dbConfig.TableName)
	rows,err := executeDbQueryWithNoResults(dbConfig,SqlStmt)
	if err != nil {
		return fmt.Errorf("Error Dropping View:%v Quetry:%v.. Error:%v\n",dbConfig.TableName,SqlStmt,err)
	}
	rows.Close()
	return nil
}


func executeDbQueryWithNoResults(dbConfig DataModels.DBConfiguration, sqlQuery string) (*sql.Rows,error) {
	db, err := ConnectToDB(dbConfig)
	if err != nil {
		// Test Failed Due to connectivity to DB
		return nil,fmt.Errorf("Error :%v", err)
	}
	defer db.Close()

	results, err := db.Query(sqlQuery)
	if err != nil {
		return nil,fmt.Errorf("Error executing Query on Tbl:%v :%v\n",dbConfig.TableName,err)
	}
	return results,nil
}

const(
	connectionTimeout=10
)

func ConnectToDB(dbConfig DataModels.DBConfiguration) (*sql.DB, error) {
	// exameple Connection string : "user:password@tcp(127.0.0.1:3306)/hello"
	// example provider : mysql

	if len(dbConfig.GcpSql) > 0 {
        cfg:= mysql.Cfg(dbConfig.GcpSql,dbConfig.UserName, dbConfig.Password)
		cfg.DBName = dbConfig.DBName
		db, err := mysql.DialCfg(cfg)
		if err != nil {
			return nil, fmt.Errorf("Error :%v", err)
		}
		// Check for Table, if not create it !!!
		return db, nil
	} else {
		connStr :=""
		switch dbConfig.Provider {
		case "mysql":
			glog.Infof("Falling back to IP Based Connectivity")
			connStr = fmt.Sprintf(DBConnectionStr, dbConfig.UserName, dbConfig.Password, dbConfig.DBHost, dbConfig.PortNumber, dbConfig.DBName)
		case "mssql","sqlserver":
			connStr = fmt.Sprintf("server=%s;user id=%s;password=%s;port=%d;database=%s", dbConfig.DBHost, dbConfig.UserName,
				dbConfig.Password, dbConfig.PortNumber, dbConfig.DBName)
		}
        glog.Infof("Connection Str:%v",connStr)
		db, err := sql.Open(dbConfig.Provider, connStr)
		if err != nil {
			return nil, fmt.Errorf("Error :%v", err)
		}
		// Open doesn't open a connection. Validate DSN data:
		err = db.Ping()
		if err != nil {
			return nil, fmt.Errorf("Error :%v", err)
		}
		// Check for Table, if not create it !!!
		return db, nil
	}
	return nil,fmt.Errorf("Unreachable Stmt")
}