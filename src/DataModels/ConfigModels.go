package DataModels

const (
	DefaultApplicationConfigFile = "estateai.yaml"
	LocalHealthChkUrl            = "http://localhost:8080/healthChk"
)

type CommonConfigForModules struct {
	ZillowApiConfig struct {
		ZillowId string `json:"Id"`
	} `json:"ZillowConfiguration"`
}

type AreaFeederConfiguration struct {
	AreaFeedConfigFile string `json:"AreaFeedConfigFile"`
	CrawlInterval      int    `json:"CrawlInterval"`
}

type EntitiesToExtract map[string]EntityExtractionDefinition

type WebCrawlerConfiguration struct {
	Url           string            `json:"Url"`
	PropertyUrl   string            `json:"PropertyUrl"`
	CrawlInterval int               `json:"CrawlInterval"`
	DUAgent       string            `json:"DUAgent"`
	Entities      EntitiesToExtract `json:"Entities"`
}

type ApiServiceConfiguration struct {
	ListenPort string `json:"ListenPort"`
}

type PropertyAnalyzerConfiguration struct {
	AreaPrefs []AreaWeigths `json:"AreaPrefs"`
}

type ApiKeysConfiguration struct {
	GoogleApiCode string `json:"GoogleApiCode"`
}

type DataHaulingConfiguration struct {
	OutpufFileName string          `json:"OutpufFileName"`
	WriteToDb      bool            `json:"WriteToDb"`
	DbConfig       DBConfiguration `json:"DbConfig"`
}

type DBConfiguration struct {
	Provider   string `json:"Provider"`
	GcpSql     string `json:"GcpSql"`
	GcpTknFile string `json:"GcpTknFile"`
	DBHost     string `json:"DBHost"`
	PortNumber int    `json:"PortNumber"`
	UserName   string `json:"UserName"`
	Password   string `json:"Password"`
	DBName     string `json:"DBName"`
	TableName  string `json:"TableName"`
}

type Config struct {
	AppCfg struct {
		LogLvl     string `json:"LogLevel"`
		MetricsUrl string `json"MetricsUrl"`
	} `json:"ApplicationConfiguration"`

	CommonConfig           CommonConfigForModules        `json:"CommonConfiguration"`
	AreaCrawlerConfig      AreaFeederConfiguration       `json:"AreaCrawlerConfig"`
	WebCrawlConfig         WebCrawlerConfiguration       `json:"WebCrawlConfig"`
	PropertyAnalyzerConfig PropertyAnalyzerConfiguration `json:"PropertyAnalyzerConfig"`
	ApiKeysConfig          ApiKeysConfiguration          `json:"ApiKeysConfig"`
	DatahaulConfig         DataHaulingConfiguration      `json:"DatahaulConfig"`
	ApiServiceConfig       ApiServiceConfiguration       `json:"ApiServiceConfig"`
}

// API Services Models
