package DataHaulingEngine

import (
	"github.com/golang/glog"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"time"
	"nrideas.visualstudio.com/EstateAI/src/MetricsAndStats"
	"nrideas.visualstudio.com/EstateAI/src/UtilLibs"
)

const (
	PeriodicStatsPeggingWriteInterval = 10 // 10 secs
)

func SetupStatsHaulEngine(dataHaulCfg DataModels.DataHaulingConfiguration){
	// Create Table /view incase they are not existing ..
	if dataHaulCfg.WriteToDb {
		UtilLibs.CreateStatsTable(dataHaulCfg.DbConfig)
		UtilLibs.CreateStatsKeyValuePairTable(dataHaulCfg.DbConfig)
	}
}

func StartStatshaulingEngine( dataHaulCfg DataModels.DataHaulingConfiguration) {
	glog.Infof("Starting Stats Hauling  Engine ")
	statsTicker := time.NewTicker(PeriodicStatsPeggingWriteInterval * time.Second).C

	for {
		select {
		case _ = <-statsTicker:
			if dataHaulCfg.WriteToDb{
				statsRecordsToInsert,statsKvPairToInsert := MetricsAndStats.GetStatsSQLInsertValuesStr()
				if len(statsRecordsToInsert) > 0 {
					go UtilLibs.ExecuteDbStatInsertQueryNow(dataHaulCfg.DbConfig, statsRecordsToInsert)
					go UtilLibs.ExecuteDbKVPairStatsInsertQueryNow(dataHaulCfg.DbConfig,statsKvPairToInsert)
				}
			}
		}
	}
}