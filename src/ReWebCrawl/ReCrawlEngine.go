package ReWebCrawl

import (
	"github.com/golang/glog"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"nrideas.visualstudio.com/EstateAI/src/IpcUtils"
	"nrideas.visualstudio.com/EstateAI/src/MetricsAndStats"
	"strings"
	"time"
)

type CrawlContext struct {
	CrawlConfig         DataModels.WebCrawlerConfiguration
	VisitedList         map[string]bool
	CarwlRequestChannel chan []string
	RedirecturlsCrawled chan string
}

func init() {
}

const (
	// Total Idle time out 15X3 = 45 secs
	CrawlIdleTimeout            = 15
	COMPLETION_THRESHHOLD_COUNT = 3
)

func KickOffCrawlEngine(cfg DataModels.WebCrawlerConfiguration, uniqueJobId int64,jobCompletetionNotificationChannel chan int64) {
	crawlCtxt := CrawlContext{}
	crawlCtxt.CrawlConfig = cfg
	crawlCtxt.VisitedList = make(map[string]bool)
	crawlCtxt.CarwlRequestChannel = make(chan []string, IpcUtils.DefaultCrawlerChannelQueueLength)
	crawlCtxt.RedirecturlsCrawled = make(chan string, IpcUtils.DefaultCrawlerChannelQueueLength)
	go crawlCtxt.StartCrawling(uniqueJobId,jobCompletetionNotificationChannel)
}

func (ctxt *CrawlContext) StartCrawling(uniqueJobId int64,jobCompletetionNotificationChannel chan int64) {
	glog.Infof(" Crawler Starting URL : %v", ctxt.CrawlConfig.Url)
	// Queue the input URL to Work Queue
	ctxt.CarwlRequestChannel <- []string{ctxt.CrawlConfig.Url}
	// Add Default Rule for Crawling HTML
	_, ok := ctxt.CrawlConfig.Entities[DataModels.LINKSKEY]
	if !ok {
		// Add links Object
		linksObject := DataModels.EntityExtractionDefinition{DataModels.QueryLinkQuery, "", DataModels.HREF}
		ctxt.CrawlConfig.Entities[DataModels.LINKSKEY] = linksObject
		//glog.Infof(" Added Default rule for crawling.. ")
	}

	// Start Idlem timer for crawler!!
	crawlIdleTicker := time.NewTicker(CrawlIdleTimeout * time.Second)
	quitFlag := false
	comepletionRetryCount := 0

	for !quitFlag {
		select {
		case inputUrls := <-ctxt.CarwlRequestChannel:
			for _, tmpurl := range inputUrls {
				ctxt.CheckCrawReadinessAndQueueforEE(tmpurl, uniqueJobId)
			}
			break
		case urlRedirected := <-ctxt.RedirecturlsCrawled:
			if !ctxt.VisitedList[urlRedirected] {
				//glog.Infof(" Marking Redirect Url:%v as Crawled", urlRedirected)
				ctxt.VisitedList[urlRedirected] = true
			}
		case elapsed := <-crawlIdleTicker.C:
			//glog.Infof("CrawlIdle Timer fired .. so we can check Crawler activty and Quit if needed : %v", elapsed)
			//glog.Infof("Queue sizes for CrawlRequest:%v, Redirect Channel:%v", len(ctxt.CarwlRequestChannel), len(ctxt.RedirecturlsCrawled))

			if len(ctxt.CarwlRequestChannel) == 0 && len(ctxt.RedirecturlsCrawled) == 0 {
				count, err := MetricsAndStats.GetInProgressCounter(uniqueJobId)
				if err != nil {
					glog.Errorf("Unbale to get counter value .. let us continue")
				} else if count == 0 {
					if comepletionRetryCount < COMPLETION_THRESHHOLD_COUNT {
						glog.Infof("No activity from Crawler,respecting thresh hold.. Current Count:%v,Limit is %v for JObID:%v, Elapsed :%v",
							comepletionRetryCount, COMPLETION_THRESHHOLD_COUNT, uniqueJobId,elapsed)
						comepletionRetryCount++
					} else {
						// Let us handle closure
						glog.Infof("No activity from Crawler , setting up to exit Crawler JOB,  ID:%v", uniqueJobId)
						//Close Channels
						close(ctxt.CarwlRequestChannel)
						close(ctxt.RedirecturlsCrawled)
						quitFlag = true
					}
				}
			}
			break

		}
	}
	// peg Stats now.
	MetricsAndStats.JobEnded(uniqueJobId)
	MetricsAndStats.RemoveJobFromMap(uniqueJobId)
	crawlIdleTicker.Stop()
	jobCompletetionNotificationChannel <- uniqueJobId
	glog.Infof("COMPLETED Crawling Job With ID: %v", uniqueJobId)
}

func (ctxt *CrawlContext) CheckCrawReadinessAndQueueforEE(rawUrl string, uniqueJobId int64) {
	trimmedUrl := strings.Split(rawUrl, "#")
	urlToCheck := trimmedUrl[0]
	isPropPage := false

	if !strings.HasPrefix(urlToCheck, ctxt.CrawlConfig.Url) {
		if !strings.HasPrefix(urlToCheck, ctxt.CrawlConfig.PropertyUrl) {
			//glog.Warningf("Skipping Parsing URI : %v since the the prefix is not the same as:%v", tmpurl, cfg.Url)
			return
		} else {
			isPropPage = true
		}
	}

	if !ctxt.VisitedList[urlToCheck] {
		ctxt.VisitedList[urlToCheck] = true
		unseenUrlObj := DataModels.UrlToCrawl{
			uniqueJobId,
			urlToCheck, ctxt.CrawlConfig.DUAgent,
			ctxt.CrawlConfig.PropertyUrl,
			isPropPage,
			ctxt.CrawlConfig.Entities,
			ctxt.CarwlRequestChannel,
			ctxt.RedirecturlsCrawled}
		IpcUtils.ChannelStore.UnseenUrls <- unseenUrlObj
	}

}
