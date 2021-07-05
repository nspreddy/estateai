package DataModels

import (
	"reflect"
	"time"
)

const (
	DefaultPropertyAnalyzerInstances = 10
	DefaultDataUploaderInstances     = 4
	Address                          = "Address"
	CityState                        = "CityState"
	Price                            = "Price"
	KeyFactors                       = "KeyFactors"
	PriceHistory                     = "PriceHistory"
	DeepPropertyFeatures             = "PropertyFeatures"
	InvalidPropType                  = "Invalid"
	SingleFamilyHome                 = "SFR"
	TownHome                         = "TownHome"
	Condo                            = "Condo"
	Land                             = "Land"
	MultiFamily                      = "MultiFamily"
	SQFTPERACRE                      = 43560
)

const (
	ZipCodeType = "zipcode"
	CityType    = "city"
	CountyType  = "county"
)

type AreaWeigths struct {
	AreaType string `json:"AreaType"`
	AreaID   string `json:"AreaID"`
	Score    int    `json:"Score"`
}

type PropertyTransactionHistory struct {
	Date  time.Time
	Price float32
	Type  string
}

const (
	Schools         = 0
	University      = 1
	ParkAndRide     = 2
	FireSataion     = 3
	CommunityCenter = 4
	ReligiousPlaces = 5
)

type NearByThings struct {
	TypeogThing  int
	NameofThings string
	Distance     float32
}

type PropertyModel struct {
	CrawlJobId     int64
	DateCrawled    string
	MLSID          string
	Address        string
	City           string
	State          string
	Country        string
	ZipCode        string
	PropertyType   string
	Price          uint
	PricePerSqft   uint
	Beds           uint
	Baths          uint
	Sqft           uint
	LotSize        uint
	ParkingSpaces  uint
	BuiltYear      uint
	DaysInMarket   uint
	ViewCount      uint
	WalkScore      uint
	Lat            float64
	Long           float64
	PropertyScore  float32
	ThingsAround   string // Json String with NearByThings structure...
	InformationSrc string
}

func (prop *PropertyModel) GetCSVFormattedStrings() []string {
	propData := reflect.ValueOf(prop)
	propElements := propData.Elem()
	return GetDataAsSlice(&propElements)
}

func (prop *PropertyModel) GetInsertRecordForSQL() string {
	propData := reflect.ValueOf(prop)
	propElements := propData.Elem()
	return GetSQLDataRecordToInsert(&propElements)
}

func CreatetableMySqlQuery(tablename string) string {
	rndprop := PropertyModel{}
	propData := reflect.ValueOf(&rndprop).Elem()
	return GetMySqlCreateTableQuery(tablename,&propData)
}

func CreatetableMsSqlQuery(tablename string) string {
	rndprop := PropertyModel{}
	propData := reflect.ValueOf(&rndprop).Elem()
	return GetMSSqlCreateTableQuery(tablename,&propData)
}

func GetPropHeader() []string {
	rndprop := PropertyModel{}
	propData := reflect.ValueOf(&rndprop).Elem()
	return GetHeader(&propData)
}
