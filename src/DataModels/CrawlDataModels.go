package DataModels

const (
	DefaultHtmlEntityExtractionEngineInstances = 100
	DefaultDUAgentInstances                    = 10
	LINKSKEY                                   = "crawlablelinks"
	QueryLinkQuery                             = "a[href]"
	HREF                                       = "href"
	TruliaAgentID                              = "Trulia"
	RedfinAgentId                              = "Redfin"
)

type EntityExtractionDefinition struct {
	Query    string `json:"Query"`
	SubQuery string `json:"SubQuery"`
	Attr     string `json:"Attr"` // Default Body of the Tag incase Attr is empty.
}

type UrlToCrawl struct {
	UniqueJobId         int64
	Url                 string
	DUAgent             string
	PropUrlPrefix       string
	IsPropertyPage      bool
	EntityDefinitions   EntitiesToExtract
	CarwlRequestChannel chan []string
	RedirecturlsCrawled chan string
}

type ProcessingRequesttoDUAgent struct {
	UniqueJobId int64
	Url         string
	Entities    map[string][]string
}
