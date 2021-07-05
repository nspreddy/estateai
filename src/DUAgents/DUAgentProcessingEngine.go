package DUAgents

import (
	"fmt"
	"github.com/golang/glog"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"nrideas.visualstudio.com/EstateAI/src/IpcUtils"
	"time"
)

func init() {

}

func StartDUAgent(DUAgent string) {

	channel, err := IpcUtils.ChannelStore.GetChannleBasedonDuAgent(DUAgent)
	if err != nil {
		glog.Warningf(" Err: %v", err)
		return
	}

	for docRequest := range channel {
		// process data based on Host Name of URL
		switch DUAgent {
		case DataModels.TruliaAgentID:
			propModel, err := processTruliaEntities(docRequest)
			if err != nil {
				glog.Warningf("Unable to Parse Document:%v", docRequest.Url, docRequest.Entities)
			} else {
				// Add Time Stamp
				currTime := time.Now()
				propModel.DateCrawled = fmt.Sprintf("%04d%02d%02d%02d", currTime.Year(), currTime.Month(), currTime.Day(), currTime.Hour())
				IpcUtils.ChannelStore.PropertyAnalyzer <- propModel
			}
			break
		case DataModels.RedfinAgentId:
			break
		}
		// get the needed DIVs
	}
}
