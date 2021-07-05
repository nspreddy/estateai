package MetricsAndStats

import (
	"testing"
	"fmt"
	"math/rand"
	"time"
)
func TestSqlQueryString(t *testing.T) {
	fmt.Printf("SQL String:%v\n",GetSqlCreateTableQuery("SampleTable"))
}

func TestInsertRecord(t *testing.T) {
	//Create a Dummy Metric
	sampleJobMetric := getRandomMetricObject()
	fmt.Printf("Rrcord:\n%v\n",sampleJobMetric.getInsertRecord())
}


func getRandomMetricObject() JobMetrics{

	randomMetric := JobMetrics{}

	randomMetric.JobId = rand.Int63()
	randomMetric.JobStart = time.Now().Unix()
	randomMetric.JobEnd = 0
	randomMetric.JobUrl="http://havefun.com"
	randomMetric.JobStatus=0
	randomMetric.MetricsJson = ""

    return randomMetric

}

