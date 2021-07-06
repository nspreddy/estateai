package main

import (
	"flag"
	"fmt"
	"github.com/golang/glog"
	"nrideas.visualstudio.com/EstateAI/src/ConfigurationUtils"
	"nrideas.visualstudio.com/EstateAI/src/DUAgents"
	"nrideas.visualstudio.com/EstateAI/src/DataHaulingEngine"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"nrideas.visualstudio.com/EstateAI/src/EstateRestApi"
	"nrideas.visualstudio.com/EstateAI/src/IpcUtils"
	"nrideas.visualstudio.com/EstateAI/src/MetricsAndStats"
	"nrideas.visualstudio.com/EstateAI/src/OrchestrationEngine"
	"nrideas.visualstudio.com/EstateAI/src/PropertyAnalyzer"
	"nrideas.visualstudio.com/EstateAI/src/ReWebCrawl"
	"nrideas.visualstudio.com/EstateAI/src/UtilLibs"
	"os"
	"time"
)

var options struct {
	config  string `validate:"nonzero"`
	jobmode bool   `validate:"nonzero"`
}

func init() {
	defaultUsage := flag.Usage
	flag.Usage = func() {
		fmt.Fprint(os.Stderr, "This service periodically generates Estate AI  ")
		defaultUsage()
	}

	flag.StringVar(&options.config, "config", DataModels.DefaultApplicationConfigFile,
		"REQUIRED: Estate AI Configuration file")
	flag.BoolVar(&options.jobmode, "jobmode", false,
		"REQUIRED: Estate AI Configuration file")


	//flag.Set("logtostderr", "false")
	flag.Set("alsologtostderr", "true")
	flag.Parse()
}

func main() {
	defer glog.Flush()
	glog.Infof(" Estate Analytics Started .. ")
	glog.Infof(" Options for this instance of application  :%v", options)
	defer glog.Flush()

	// Start reading Configuration file.
	config, err := ConfigurationUtils.LoadConfiguration(options.config)
	if err != nil {
		glog.Fatalf("Unable to load/parse configuration file, Error : %v", err)
		return
	}

	glog.Infof("Configuration Object\n:%v", config)

	/**
	 **  Area feeder feeds Lat/long or address and Radius based configuration for Property Crawler.
	 */
	// Kick off Area feeder
	// go AreaFeeder.StartAreaFeeder(config.CommonConfig,config.AreaCrawlerConfig, IpcUtils.ChannelStore.AreaCrawlChannel)

	// make  instances of crawl Links and Entity Extractors
	glog.Infof("Setting up Entity Extraction engines")

	for i := 1; i < DataModels.DefaultHtmlEntityExtractionEngineInstances; i++ {
		go ReWebCrawl.CrawlAndExtractEntities()
	}

	glog.Infof("Setting up Document Understanding Agents")
	// Kick off DU Agent Engines
	for i := 1; i < DataModels.DefaultDUAgentInstances; i++ {
		go DUAgents.StartDUAgent(config.WebCrawlConfig.DUAgent)
	}

	glog.Infof("Setting up Property Analyzers.")
	for i := 1; i < DataModels.DefaultPropertyAnalyzerInstances; i++ {
		//kick off property analysis engine
		go PropertyAnalyzer.StartPropertyAnalyzerEngine(config.PropertyAnalyzerConfig, config.ApiKeysConfig)
	}

	// Setup Cloud Proxy
	err = UtilLibs.InitDBClientAndProxy(config.DatahaulConfig)
	if err != nil {
       glog.Fatalf("Unable to init cloud proxy:%v, which is needed to talk to DB, you can disable writing to DB",err)
	}

     glog.Infof("Setting up Data Hauling engine")
	// Setup DataHaul Engine prior to staring its instances
	DataHaulingEngine.SetupDataHaulEngine(config.DatahaulConfig)

	for i := 1; i < DataModels.DefaultDataUploaderInstances; i++ {
		//kick off data uploaders
		go DataHaulingEngine.StartDataHaulEngine(config.ApiKeysConfig, config.DatahaulConfig)
	}

	glog.Infof("Setting up Stats Hauling engine")
	// Setup Stats Hauling engine
	DataHaulingEngine.SetupStatsHaulEngine(config.DatahaulConfig)
	// Time to kick off Stats hauling Engine
	go DataHaulingEngine.StartStatshaulingEngine(config.DatahaulConfig)

	glog.Infof("Starting up Orchestration engine..")
	// Kick off Orchestration engine
	go OrchestrationEngine.StartOrchestrationEngine()

    glog.Infof("Starting up API Endpoint")
	// Let us kick off Web API
	go EstateRestApi.StartRestApiService(config.CommonConfig, config.ApiServiceConfig)

	glog.Infof("Starting up Job Scheduler engine..")
	// Queue workitems based on configuration
	// IpcUtils.ChannelStore.OrchestrationWebCrawlChannel <- config.WebCrawlConfig
	// Start Config based Job Scheduler
	go StartConfigBasedJobScheduler(config.WebCrawlConfig)

	// Kick off Health Checker every 5 secs
	performHealthChecks(config.DatahaulConfig)

}

func StartConfigBasedJobScheduler(webconfig DataModels.WebCrawlerConfiguration) {
	// create a timer based on CrawlInterval
	// CrawlInterval in mins
	requeueTimer := time.NewTicker( time.Duration(webconfig.CrawlInterval) *time.Minute).C
	glog.Infof(" READY to RUN THE SHOW.. ")
	glog.Infof("------------- START , CAMERA , ACTION ----------------\n")

	for{
		glog.Infof("Queuing Work Item : %v ",webconfig.Url)
		IpcUtils.ChannelStore.OrchestrationWebCrawlChannel <- webconfig
		glog.Infof("Waiting for %v mins to Queue next work Item",webconfig.CrawlInterval)
		<- requeueTimer
	}
}

// Health Check and Write Metrics to DB incase DB flag is on.
func performHealthChecks(dataHaulCfg DataModels.DataHaulingConfiguration) {

	// make sure StatsTable is created
	sleepTicker := time.NewTicker(30 * time.Second).C

	for {
		select {
		case elapsed :=  <-sleepTicker:
			metricsStr := MetricsAndStats.GetJsonString()
			glog.Infof("********************Stats Context*********************\n")
			glog.Infof("Elapsed:%v, %v\n",elapsed,metricsStr)
			glog.Infof("JobMode:%v",options.jobmode)
		case completedJobId := <-IpcUtils.ChannelStore.JobstatusNotificationChannel:
			glog.Infof("Received Notification about Job Completetion.., JobId:%v", completedJobId)
			if options.jobmode {
				glog.Infof("Completed Job, based on cmdline param jobmode being True.. Exiting the game")
				return
			}
		}
	}

	/*for range sleepTicker {
		glog.Infof("Performing Health Checks @ %v",time.Now())
		res, err := http.Get(DataModels.LocalHealthChkUrl)
		if err != nil {
			glog.Fatal("Failed to perform Health Check (Uri : %v), Error: %v", DataModels.LocalHealthChkUrl, err)
			continue
		}
		 data, err := ioutil.ReadAll(res.Body)
		 res.Body.Close()

		 if err != nil {
			glog.Errorf("Unable to read response from heath Endpoint:%v",DataModels.LocalHealthChkUrl)
			continue
		}
		hlthResult := DataModels.HealthCheck{}
		err = json.Unmarshal(data, &hlthResult)
		if err != nil {
			glog.Errorf("Unable to parse response(%v) from heath Endpoint:%v",string(data),DataModels.LocalHealthChkUrl)
			continue
		}

		if hlthResult.LiveCheck {
			glog.Infof("Healthy End point : %v, JSON Response %v",DataModels.LocalHealthChkUrl,hlthResult)
		} else {
			glog.Errorf("UnHealthy End point : %v, Reason : %v",DataModels.LocalHealthChkUrl,hlthResult)
		}
	}*/


}
