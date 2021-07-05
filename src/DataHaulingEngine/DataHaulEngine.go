package DataHaulingEngine

import (
	"fmt"
	"github.com/golang/glog"
	"math/rand"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"nrideas.visualstudio.com/EstateAI/src/IpcUtils"
	"time"
	"nrideas.visualstudio.com/EstateAI/src/UtilLibs"
)

func init() {
}

const (
	PeriodicWriteInterval = 15 // 15 secs
)

func SetupDataHaulEngine(dataHaulCfg DataModels.DataHaulingConfiguration){
	// Create Table /view incase they are not existing ..
	if dataHaulCfg.WriteToDb {
		UtilLibs.CreateTableAndView(dataHaulCfg.DbConfig)
	}
}

func StartDataHaulEngine(apikeyCfg DataModels.ApiKeysConfiguration, dataHaulCfg DataModels.DataHaulingConfiguration) {
	uniqueId := fmt.Sprintf("haulingEngine_%d", rand.Int63())
	// Init Data Store
	dataBuf := CreateDataBuffStore(dataHaulCfg.OutpufFileName, uniqueId)
	sleepTicker := time.NewTicker(PeriodicWriteInterval * time.Second).C

	for {
		isReadyToHaulOff := false
		select {
		case propToAnalyze := <-IpcUtils.ChannelStore.DataUploaderChannel:
			dataBuf.AddpropertyObject(&propToAnalyze)
			if dataBuf.IsFull() {
				isReadyToHaulOff = true
			}
		case _ = <-sleepTicker:
			//glog.Infof("Data Hauler timer Fired:%v",elapsed)
			isReadyToHaulOff = true
		}

		// Set flag to haul off
		if isReadyToHaulOff {
			err := dataBuf.WriteToFile()
			if err != nil {
				glog.Errorf("Unable to write to File , error :%v", err)
			}

			if dataHaulCfg.WriteToDb{
				err := dataBuf.WriteToDB(dataHaulCfg.DbConfig)
				if err != nil {
					glog.Errorf("Unable to write to DB due to :%v", err)
				}
			} else {
				//glog.Infof("Skipping to write to DB engine @ %v, Database : %v",dataHaulCfg.DbConfig.DBHost,dataHaulCfg.DbConfig.DBName)
			}
			// get new Data Buff, golang will recycle previous objects
			dataBuf = CreateDataBuffStore(dataHaulCfg.OutpufFileName, uniqueId)
		}
	}
}

