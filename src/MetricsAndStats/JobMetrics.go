package MetricsAndStats

import (
	"sync"
	"time"
	"fmt"
	"encoding/json"
	"reflect"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"github.com/golang/glog"
)

const (
	CrawlCount               = "CrawlCount"
	PropertyCount            = "PropertyCount"
	LessThanQtr              = "LessThanQtrMillion"
	QtrToHalf                = "QtrToHalfMillion"
	HalfToMil                = "HalfToMillion"
	MilToTwoMil              = "MillionTo2Million"
	AboveTwoMil              = "Above2Million"
	Before1990               = "before1990"
	After1990Before2000      = "After1990Before2000"
	After2000Before2010      = "After2000Before2010"
	After2010                = "After2010"
	LessThan1Acre            = "LessThan_1_Acre"
	LessThan5AcreMoreThan1   = "LessThan_5_Acre_Morethan_1"
	LessThan10AcreMorethan1  = "LessThan_10_Acre_Morethan_5"
	LessThan20AcreMorethan10 = "LessThan_20_Acre_Morethan_10"
	Morethan20               = "Morethan_20"
)

type JobMetricsLockObject struct {
	sync.Mutex
	purpose string
}

var jobMetricsLock *JobMetricsLockObject

type MetricKeyValueMap struct {
	 KeyValuePair map[string]int
}

type JobMetrics struct {
	JobId                      int64
	JobUrl                     string
	JobStartStr                string
	JobStart                   int64
	JobEnd                     int64
	JobDuration                int64
	JobFatalErrorCount         int
	JobErrorCount              int
	JobWarningCount            int
	JobStatus                  int // 0- Sucess , non Zero with error code (TBD)
	TotalDataRecordsCount      int
	TotalValidProcessedRecords int
}

type JobStatsKeyValuePair struct {
	JobId                      int64
	TimeStamp                  int64
	KeyName                    string
	Value                      int
}


var jobMetircsMap                   map[int64]JobMetrics
var jobMetricsKeyValuePairsMap      map[int64]MetricKeyValueMap

func init() {
	// init Channels
	jobMetircsMap                = make(map[int64]JobMetrics)
	jobMetricsKeyValuePairsMap   = make(map[int64]MetricKeyValueMap)
	jobMetricsLock               = &JobMetricsLockObject{purpose: "JobMetricsLock"}
}

// Stats pegging utils
func StartJob(jobId int64, jobUrl string){
	jobMetricsLock.Lock()
	defer jobMetricsLock.Unlock()

	jobMetric := getJobMetrics(jobId)
	jobMetric.JobId = jobId
	jobMetric.JobUrl = jobUrl
	jobMetircsMap[jobId] = jobMetric
}

func JobEnded(jobId int64){
	jobMetricsLock.Lock()
	defer jobMetricsLock.Unlock()
	jobMetric := getJobMetrics(jobId)
	jobMetric.JobEnd = time.Now().Unix()
	// compute Duration now in seconds
	jobMetric.JobDuration = jobMetric.JobEnd - jobMetric.JobStart
	jobMetircsMap[jobId] = jobMetric
}

func IncrementFatalErrorCount(jobId int64){
	jobMetricsLock.Lock()
	defer jobMetricsLock.Unlock()

	jobMetric := getJobMetrics(jobId)
	jobMetric.JobFatalErrorCount++
	jobMetircsMap[jobId] = jobMetric
}

func IncremenErrorCount(jobId int64){
	jobMetricsLock.Lock()
	defer jobMetricsLock.Unlock()

	jobMetric := getJobMetrics(jobId)
	jobMetric.JobErrorCount++
	jobMetircsMap[jobId] = jobMetric
}

func IncrementWarningCount(jobId int64){
	jobMetricsLock.Lock()
	defer jobMetricsLock.Unlock()

	jobMetric := getJobMetrics(jobId)
	jobMetric.JobWarningCount++
	jobMetircsMap[jobId] = jobMetric
}

func IncrementTotalRecords(jobId int64){
	jobMetricsLock.Lock()
	defer jobMetricsLock.Unlock()

	jobMetric := getJobMetrics(jobId)
	jobMetric.TotalDataRecordsCount++
	jobMetircsMap[jobId] = jobMetric
}

func IncrementProcessedValidRecords(jobId int64){
	jobMetricsLock.Lock()
	defer jobMetricsLock.Unlock()

	jobMetric := getJobMetrics(jobId)
	jobMetric.TotalValidProcessedRecords++
	jobMetircsMap[jobId] = jobMetric
}

func IncrementJobMetric(jobId int64,key string) {
	jobMetricsLock.Lock()
	defer jobMetricsLock.Unlock()
	jobKeyValueMetric := getJobKeyValuePair(jobId)
	jobKeyValueMetric.increment(key)
}

func DecrementJobMetric(jobId int64,key string) {
	jobMetricsLock.Lock()
	defer jobMetricsLock.Unlock()
	jobKeyValueMetric := getJobKeyValuePair(jobId)
	jobKeyValueMetric.decrement(key)

}

func  GetCount(jobId int64,key string) (int, error) {
	jobMetricsLock.Lock()
	defer jobMetricsLock.Unlock()
	jobKeyValueMetric := getJobKeyValuePair(jobId)
	return jobKeyValueMetric.getCount(key)
}

func GetJsonString() string {
	jobMetricsLock.Lock()
	defer jobMetricsLock.Unlock()
	jsonBytes, err := json.Marshal(jobMetircsMap)
	returnStr := ""
	if err != nil {
		returnStr = fmt.Sprintf("Err:%v", err)
	} else {
		returnStr = string(jsonBytes)
	}
	return returnStr
}

func  WriteStatsToEncoder(encoder *json.Encoder) {
	jobMetricsLock.Lock()
	defer jobMetricsLock.Unlock()
	encoder.Encode(jobMetircsMap)
}


func GetStatsSQLInsertValuesStr() (string,string) {
	jobMetricsLock.Lock()
	defer jobMetricsLock.Unlock()

	resultedValuesStr := ""
	isFirstElement := true
	totalRecordsToWrite := 0
	var jobsToRemove []int64

	for key, metricObj := range jobMetircsMap {
		if isFirstElement {
			resultedValuesStr = fmt.Sprintf("%v", metricObj.getInsertRecord())
			isFirstElement = false
		} else {
			resultedValuesStr = fmt.Sprintf("%v,%v", resultedValuesStr, metricObj.getInsertRecord())
		}
		if metricObj.JobEnd != 0 {
			jobsToRemove = append(jobsToRemove,key)
		}
		totalRecordsToWrite++
	}
	// get KV Pair Stuff
	kvpairStatValues := getStatsKVPairInsertValueStr()
	// Clean up now Jobs which are ended
	for _,key := range jobsToRemove {
		glog.Infof("Complete Job: %v, removing Metrics Object and KVpair Objects from maps",key)
		delete(jobMetircsMap,key)
		delete(jobMetricsKeyValuePairsMap,key)
	}
	return resultedValuesStr,kvpairStatValues
}

func getStatsKVPairInsertValueStr() string {
	resultedValuesStr := ""
	isFirstElement := true

	for  jobId, kvpairObj := range jobMetricsKeyValuePairsMap {
		kvPairMap := kvpairObj.KeyValuePair
        for key,metricvalue := range kvPairMap {
        	kvpairDbRecord := JobStatsKeyValuePair{jobId,time.Now().Unix(),key,metricvalue}
			if isFirstElement {
				resultedValuesStr = fmt.Sprintf("%v", kvpairDbRecord.getInsertRecord())
				isFirstElement = false
			} else {
				resultedValuesStr = fmt.Sprintf("%v,%v", resultedValuesStr, kvpairDbRecord.getInsertRecord())
			}
		}
	}
	return resultedValuesStr
}

// SQL Query related utils
func GetSqlCreateTableQuery(tablename string) string {
	jobMetricsObject := JobMetrics {}
	propData := reflect.ValueOf(&jobMetricsObject).Elem()
	sqlCreateStr := DataModels.GetMySqlCreateTableQuery(tablename,&propData)
	return sqlCreateStr
}

// Get SQL KV Pair
func GetSqlKVPairCreateTableQuery(tablename string) string {
	jobKVPair := JobStatsKeyValuePair {}
	propData := reflect.ValueOf(&jobKVPair).Elem()
	sqlCreateStr := DataModels.GetMySqlCreateTableQuery(tablename,&propData)
	return sqlCreateStr
}

func (kvdbObj *JobStatsKeyValuePair) getInsertRecord() string{
	metricData := reflect.ValueOf(kvdbObj)
	metricsElements := metricData.Elem()
	return DataModels.GetSQLDataRecordToInsert(&metricsElements)
}

// Helper Methods, not to be shared with other packages
func ( metrisObj *JobMetrics) getInsertRecord() string{
	metricData := reflect.ValueOf(metrisObj)
	metricsElements := metricData.Elem()
	return DataModels.GetSQLDataRecordToInsert(&metricsElements)
}

// Private methods, no exposed out side of package
func getJobMetrics(jobId int64) JobMetrics{
	value, ok := jobMetircsMap[jobId]
	if !ok {
		value = JobMetrics{}
		currTime := time.Now()
		value.JobStart    = currTime.Unix()
		value.JobStartStr =  fmt.Sprintf("%04d%02d%02d%02d%2d%2d", currTime.Year(), currTime.Month(), currTime.Day(), currTime.Hour(),currTime.Minute(),currTime.Second())
		jobMetircsMap[jobId] = value
	}
	return value
}

func getJobKeyValuePair(jobId int64) MetricKeyValueMap{
	value, ok := jobMetricsKeyValuePairsMap[jobId]
	if !ok {
		value = MetricKeyValueMap{}
		value.KeyValuePair = make(map[string]int)
		jobMetricsKeyValuePairsMap[jobId] = value
	}
	return value
}

func ( keyvalmapObj *MetricKeyValueMap) getJsonStr() string {
	jsonBytes, err := json.Marshal(keyvalmapObj)
	returnStr := ""
	if err != nil {
		returnStr = fmt.Sprintf("Err:%v", err)
	} else {
		returnStr = string(jsonBytes)
	}
	return returnStr
}

func ( keyvalmapObj *MetricKeyValueMap) getCount(key string) (int,error) {
	value, ok := keyvalmapObj.KeyValuePair[key]
	if !ok {
		return 0, fmt.Errorf("Unable to find the Key:%v",key)
	}
	return value,nil
}

func ( keyvalmapObj *MetricKeyValueMap) increment(key string) {
	value, ok := keyvalmapObj.KeyValuePair[key]
	if !ok {
		keyvalmapObj.KeyValuePair[key] = 1
	} else {
		value++
		keyvalmapObj.KeyValuePair[key] = value
	}
}

func ( keyvalmapObj *MetricKeyValueMap) decrement(key string) {
	value, ok := keyvalmapObj.KeyValuePair[key]
	if !ok {
		keyvalmapObj.KeyValuePair[key] = 0
	} else {
		if value >0 {
			value--
			keyvalmapObj.KeyValuePair[key] = value
		}
	}
}








