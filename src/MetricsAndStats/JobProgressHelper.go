package MetricsAndStats

import (
	"fmt"
	"sync"
	"github.com/golang/glog"
)

type LockResource struct {
	sync.Mutex
	purpose string
}

var jobProgressLock *LockResource

type JobProgressCounters struct {
	InProgressCounter int
}

var jobProgressMap map[int64]JobProgressCounters

func init() {
	// init Channels
	jobProgressMap = make(map[int64]JobProgressCounters)
	jobProgressLock = &LockResource{purpose: "JobProgressLock"}
}

func IncrementProgressCounter(jobId int64) {
	jobProgressLock.Lock()
	defer jobProgressLock.Unlock()

	value, ok := jobProgressMap[jobId]
	if !ok {
		jobProgressMap[jobId] = JobProgressCounters{1}
	} else {
		value.InProgressCounter += 1
		jobProgressMap[jobId] = value
	}
}

func DecrementProgressCounter(jobId int64) {
	jobProgressLock.Lock()
	defer jobProgressLock.Unlock()

	value, ok := jobProgressMap[jobId]
	if !ok {
		jobProgressMap[jobId] = JobProgressCounters{0}
	} else {
		value.InProgressCounter -= 1
		jobProgressMap[jobId] = value
	}
}

func GetInProgressCounter(jobId int64) (int, error) {
	jobProgressLock.Lock()
	defer jobProgressLock.Unlock()

	value, ok := jobProgressMap[jobId]
	if !ok {
		return -1, fmt.Errorf("Job:%v missing")
	}
	return value.InProgressCounter, nil
}

func SetInProgressCounter(jobId int64, count int) {
	jobProgressLock.Lock()
	defer jobProgressLock.Unlock()

	value, ok := jobProgressMap[jobId]
	if !ok {
		jobProgressMap[jobId] = JobProgressCounters{count}
	} else {
		value.InProgressCounter = count
		jobProgressMap[jobId] = value
	}
}

func RemoveJobFromMap(jobId int64){
	jobProgressLock.Lock()
	defer jobProgressLock.Unlock()

	_, ok := jobProgressMap[jobId]
	if ok {
	    // remove Job Session object
	    glog.Infof("Removing Job Session with ID :%d since Job Completed",jobId)
	    delete(jobProgressMap,jobId)
	}

}
