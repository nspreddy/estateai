package ReWebCrawl

import (
	"fmt"
	"github.com/PuerkitoBio/goquery"
	"github.com/golang/glog"
	"golang.org/x/net/html"
	"net/http"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"nrideas.visualstudio.com/EstateAI/src/IpcUtils"
	"nrideas.visualstudio.com/EstateAI/src/MetricsAndStats"
	"strings"
)

// Read the queue of un seen Urls and process the Crawling URL Job.
func CrawlAndExtractEntities() {
	for urlObject := range IpcUtils.ChannelStore.UnseenUrls {

		MetricsAndStats.IncrementProgressCounter(urlObject.UniqueJobId)
		foundLinks, err := processUrlAndgetLinks(urlObject)
		MetricsAndStats.DecrementProgressCounter(urlObject.UniqueJobId)

		if err != nil {
			glog.Warningf("Error extracting Url : %v", err)
			continue
		}
		go func() {
			urlObject.CarwlRequestChannel <- foundLinks
		}()
	}
}

// Extract makes an HTTP GET request to the specified URL, parses
// the response as HTML, and returns the links in the HTML document.
func processUrlAndgetLinks(urlObj DataModels.UrlToCrawl) ([]string, error) {

	client := &http.Client{}
	req, err := http.NewRequest("GET", urlObj.Url, nil)
	if err != nil {
		return nil, fmt.Errorf("error creating new request %s:%s", urlObj.Url, err)
	}

	req.Header.Set("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.75 Safari/537.36")
	req.Header.Set("User-Agent", "BingBot")
	//req.Header.Set("accept","text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8")

	resp, err := client.Do(req)

	if err != nil {
		return nil, err
	}
	if resp.StatusCode != http.StatusOK {
		resp.Body.Close()
		return nil, fmt.Errorf("getting %s: %s", urlObj.Url, resp.Status)
	}

	finalUrl := resp.Request.URL.String()
	if !urlObj.IsPropertyPage {
		if strings.Compare(urlObj.Url, finalUrl) != 0 {
			// Could be a redirect scenario.
			if strings.HasPrefix(finalUrl, urlObj.PropUrlPrefix) {
				urlObj.IsPropertyPage = true
				go func() { urlObj.RedirecturlsCrawled <- finalUrl }()
			}
		}
	}

	doc, err := html.Parse(resp.Body)
	resp.Body.Close()
	if err != nil {
		return nil, fmt.Errorf("Error parsing %s as HTML: %v", urlObj.Url, err)
	}

	//var links []string
	var entities map[string][]string
	entities = make(map[string][]string)

	extractEntities(doc, urlObj.EntityDefinitions, entities)

	if urlObj.IsPropertyPage {
		//ToDo: This is property page, Queue Data  to extract entities
		MetricsAndStats.IncrementJobMetric(urlObj.UniqueJobId,MetricsAndStats.PropertyCount)
		MetricsAndStats.IncrementProcessedValidRecords(urlObj.UniqueJobId)
		docProcessingRequest := DataModels.ProcessingRequesttoDUAgent{urlObj.UniqueJobId, finalUrl, entities}
		// Queue Request to a Channel
		//IpcUtils.ChannelStore.HtmlDocProcssingChannel <- docProcessingRequest
		channel, err := IpcUtils.ChannelStore.GetChannleBasedonDuAgent(urlObj.DUAgent)

		if err != nil {
			glog.Warningf(" Err: %v", err)
		} else {
			channel <- docProcessingRequest
		}
	}
	MetricsAndStats.IncrementJobMetric(urlObj.UniqueJobId,MetricsAndStats.CrawlCount)
	MetricsAndStats.IncrementTotalRecords(urlObj.UniqueJobId)

	var links = entities[DataModels.LINKSKEY]
	var crawlableLinks []string
	for _, rawRefLink := range links {
		link, err := resp.Request.URL.Parse(rawRefLink)
		if err == nil {
			crawlableLinks = append(crawlableLinks, link.String())
		}
	}
	return crawlableLinks, nil
}

// Extract Entities from given definition
func extractEntities(node *html.Node, entityDef DataModels.EntitiesToExtract, resultMap map[string][]string) {
	doc := goquery.NewDocumentFromNode(node)
	for key, value := range entityDef {
		extractedEntities := executeGoQuery(doc, value)
		for _, extractedEntityStr := range extractedEntities {
			updateMap(resultMap, key, extractedEntityStr)
		}
	}
}

// execute query across HTML Doc
func executeGoQuery(doc *goquery.Document, queryDef DataModels.EntityExtractionDefinition) []string {
	extractedData := make([]string, 0)
	query := strings.TrimSpace(queryDef.Query)
	// Get nodes
	doc.Find(query).Each(func(i int, s *goquery.Selection) {
		extractedEntityStr := ""
		if len(strings.TrimSpace(queryDef.SubQuery)) > 0 {
			extractedEntityStr = executeSubQuery(s, queryDef.SubQuery, queryDef.Attr)
		} else if len(strings.TrimSpace(queryDef.Attr)) > 0 {
			entityData, ok := s.Attr(queryDef.Attr)
			if ok {
				extractedEntityStr = entityData
			}
		} else {
			// get the Text part of Body
			extractedEntityStr = s.Text()
		}
		trimmedStr := strings.TrimSpace(extractedEntityStr)
		if len(trimmedStr) > 0 {
			extractedData = append(extractedData, strings.TrimSpace(trimmedStr))
		}
	})

	return extractedData
}

// Execute query (Jquery style)  on a given HTML Node
func executeSubQuery(doc *goquery.Selection, query string, attr string) string {
	extractedEntityDataStr := ""

	doc.Find(query).Each(func(i int, s *goquery.Selection) {
		tmpStr := ""
		if len(strings.TrimSpace(attr)) > 0 {
			entityData, ok := s.Attr(attr)
			if ok {
				tmpStr = entityData
			}
		} else {
			// get the Text part of Body
			tmpStr = s.Text()
		}

		if len(extractedEntityDataStr) == 0 {
			extractedEntityDataStr = fmt.Sprintf("%s", strings.TrimSpace(tmpStr))
		} else {
			extractedEntityDataStr = fmt.Sprintf("%s_%s", extractedEntityDataStr, strings.TrimSpace(tmpStr))
		}

	})
	return extractedEntityDataStr
}

// Utility function to update map
func updateMap(datamap map[string][]string, key string, value string) {
	mapelement, ok := datamap[key]
	if !ok {
		valuelist := make([]string, 5)
		datamap[key] = append(valuelist, value)
	} else {
		datamap[key] = append(mapelement, value)
	}
}
