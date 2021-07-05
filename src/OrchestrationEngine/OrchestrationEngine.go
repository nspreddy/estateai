package OrchestrationEngine

import (
	"nrideas.visualstudio.com/EstateAI/src/IpcUtils"
	"nrideas.visualstudio.com/EstateAI/src/MetricsAndStats"
	"nrideas.visualstudio.com/EstateAI/src/ReWebCrawl"
	"time"
)


func init() {
}

func StartOrchestrationEngine() {

	//glog.Infof("Starting Orchestration engine")
	// Kicking off crawlers based on configuration
	for {
		select {
		case webCrawlRequest := <-IpcUtils.ChannelStore.OrchestrationWebCrawlChannel:
			// Compute Kick off Instance ID ( Unique ID per run )
			uniqueJobId := time.Now().Unix()
			MetricsAndStats.SetInProgressCounter(uniqueJobId, 0)
			MetricsAndStats.StartJob(uniqueJobId,webCrawlRequest.Url)
			go ReWebCrawl.KickOffCrawlEngine(webCrawlRequest, uniqueJobId,IpcUtils.ChannelStore.JobstatusNotificationChannel)
			break
		}
	}
}
