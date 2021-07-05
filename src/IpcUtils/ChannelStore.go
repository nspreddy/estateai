package IpcUtils

import (
	"fmt"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
)

type Channels struct {
	AreaCrawlChannel             chan DataModels.AreaDefinition
	OrchestrationWebCrawlChannel chan DataModels.WebCrawlerConfiguration
	UnseenUrls                   chan DataModels.UrlToCrawl
	PropertyAnalyzer             chan DataModels.PropertyModel
	DataUploaderChannel          chan DataModels.PropertyModel
	duAgentChannels              map[string]chan DataModels.ProcessingRequesttoDUAgent
	JobstatusNotificationChannel chan int64
}

const (
	DefaultChannelQueueSize          = 10
	DefaultCrawlerChannelQueueLength = 100
	DataUploaderChannelQueueSize     = 1000
)

var ChannelStore Channels

var duAgentList = []string{DataModels.TruliaAgentID, DataModels.RedfinAgentId}

func init() {
	// init Channels
	ChannelStore.AreaCrawlChannel = make(chan DataModels.AreaDefinition, DefaultChannelQueueSize)
	ChannelStore.OrchestrationWebCrawlChannel = make(chan DataModels.WebCrawlerConfiguration, DefaultChannelQueueSize)
	ChannelStore.PropertyAnalyzer = make(chan DataModels.PropertyModel, DefaultChannelQueueSize)
	ChannelStore.DataUploaderChannel = make(chan DataModels.PropertyModel, DataUploaderChannelQueueSize)
	ChannelStore.UnseenUrls = make(chan DataModels.UrlToCrawl, DefaultCrawlerChannelQueueLength)
	ChannelStore.duAgentChannels = make(map[string]chan DataModels.ProcessingRequesttoDUAgent)
	ChannelStore.JobstatusNotificationChannel = make( chan int64,DefaultCrawlerChannelQueueLength)

	//make channels for providers
	for _, duAgent := range duAgentList {
		ChannelStore.duAgentChannels[duAgent] = make(chan DataModels.ProcessingRequesttoDUAgent, DefaultCrawlerChannelQueueLength)
	}
}

func (channelStore *Channels) GetChannleBasedonDuAgent(duAgent string) (chan DataModels.ProcessingRequesttoDUAgent, error) {
	value, ok := channelStore.duAgentChannels[duAgent]
	if !ok {
		return nil, fmt.Errorf("Unable to find Channel for given DU Agent:%v", duAgent)
	}
	return value, nil
}
