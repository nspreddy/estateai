package DUAgents

import (
	"fmt"
	"github.com/golang/glog"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"regexp"
	"strconv"
	"strings"
)

func processTruliaEntities(docRequest DataModels.ProcessingRequesttoDUAgent) (DataModels.PropertyModel, error) {

	propDetails := DataModels.PropertyModel{}
	propDetails.InformationSrc = "TruliaDU"
	propDetails.CrawlJobId = docRequest.UniqueJobId
	for key, data := range docRequest.Entities {

		if len(data) == 0 {
			continue
		}

		switch key {
		case DataModels.LINKSKEY:
			break
		case DataModels.Address:
			err := parseAddress(&propDetails, data)
			if err != nil {
				glog.Errorf("%v", err)
			}
			break
		case DataModels.CityState:
			err := parseCityData(&propDetails, data)
			if err != nil {
				glog.Errorf("%v", err)
			}
		case DataModels.Price:
			err := parsePrice(&propDetails, data)
			if err != nil {
				glog.Errorf("%v", err)
			}
		case DataModels.KeyFactors:
			parsePropFeatures(&propDetails, data)
		case DataModels.DeepPropertyFeatures:
			parsePropFeatures(&propDetails, data)
		case DataModels.PriceHistory:
			//ToDo: TBD
		}
	}
	//glog.Infof("Doc URI:%v\nParsed Doc :%v",docRequest.Url,propDetails)
	return propDetails, nil
}

func parseAddress(property *DataModels.PropertyModel, features []string) error {
	//glog.Infof("Price RAW String:%v, Length:%d",features,len(features))
	for _, addrRawStr := range features {
		trimmedrawStr := strings.TrimSpace(addrRawStr)
		if len(trimmedrawStr) == 0 {
			continue
		}
		property.Address = trimmedrawStr
		break
	}

	return nil
}

func parseCityData(property *DataModels.PropertyModel, features []string) error {
	//glog.Infof("Price RAW String:%v, Length:%d",features,len(features))
	for _, cityStr := range features {
		trimmedrawStr := strings.TrimSpace(cityStr)
		if len(trimmedrawStr) == 0 {
			continue
		}
		property.City = trimmedrawStr

		//ToDo: Parse City,State and Zip Code
		regex := `\s*(?P<City>\D+)\s*,\s*(?P<State>\D+)(?P<ZipCode>\d+)`
		params := getParams(regex, trimmedrawStr)

		cityStr, ok := params["City"]
		if !ok {
			return fmt.Errorf("CityName Not present")
		}
		property.City = cityStr

		stateStr, ok := params["State"]
		if !ok {
			return fmt.Errorf("State Not present")
		}
		property.State = stateStr

		zipStr, ok := params["ZipCode"]
		if !ok {
			return fmt.Errorf("ZipCode Not present")
		}
		property.ZipCode = zipStr

		property.Country = "USA"

		break
	}

	return nil
}

func parsePrice(property *DataModels.PropertyModel, features []string) error {
	//glog.Infof("Price RAW String:%v, Length:%d",features,len(features))
	re := regexp.MustCompile("\\$|,")
	property.Price = 0
	for _, priceRawStr := range features {
		trimmedrawStr := strings.TrimSpace(priceRawStr)
		if len(trimmedrawStr) == 0 {
			continue
		}
		resultStr := re.ReplaceAllString(trimmedrawStr, "")
		//glog.Infof("Raw String: %v, Result Str :%v",trimmedrawStr,resultStr)
		result, err := strconv.ParseInt(resultStr, 10, 64)
		if err != nil {
			continue
		}
		property.Price = uint(result)
		break
	}
	if property.Price == 0 {
		return fmt.Errorf("Unable to get the price from Given Data : %v", features)
	}
	return nil
}

func parsePropFeatures(property *DataModels.PropertyModel, features []string) error {

	for _, featureRawStr := range features {
		//glog.Infof(" About to Process %v:%v and Prop Details ",featureStr,property)
		// property Type
		featureStr := strings.TrimSpace(featureRawStr)
		if len(featureStr) == 0 {
			continue
		}
		//glog.Infof("Formatted Input Str:%v",featureStr)
		propType, err := parseForPropType(featureStr)
		if err == nil {
			property.PropertyType = propType
			continue
		}

		// check for beds
		beds, err := parseBeds(featureStr)
		if err == nil {
			property.Beds = beds
			continue
		}

		// Check for Baths
		baths, err := parseBaths(featureStr)
		if err == nil {
			property.Baths = baths
			continue
		}

		// Check for Sqft
		sqft, err := parseSqft(featureStr)
		if err == nil {
			property.Sqft = sqft
			continue
		}

		// check for Mls #
		mlsid, err := parseMls(featureStr)
		if err == nil {
			property.MLSID = mlsid
			continue
		}

		//Lot Size SQFT/Acers
		lotSqft, err := parseLotSize(featureStr)
		if err == nil {
			property.LotSize = lotSqft
			continue
		}

		// Days Listed
		daysInmarket, err := parseDaysInMarket(featureStr)
		if err == nil {
			property.DaysInMarket = daysInmarket
			continue
		}
		// Views
		viewCount, err := parseViewCount(featureStr)
		if err == nil {
			property.ViewCount = viewCount
			continue
		}

		// Built Year
		builtYr, err := parseBuiltYear(featureStr)
		if err == nil {
			property.BuiltYear = builtYr
			continue
		}

		parkingSpaces, err := parseParkingSpaces(featureStr)
		if err == nil {
			property.ParkingSpaces = parkingSpaces
			continue
		}

	}

	return nil
}

func parseBeds(input string) (uint, error) {
	regex := `^\s*(?P<Count>\d+)\s*(?P<Type>Beds|Bed\s*rooms)`
	trimmedStr := strings.TrimSpace(input)
	params := getParams(regex, trimmedStr)
	bedsStr, ok := params["Count"]
	if !ok {
		return 0, fmt.Errorf("Bedrooms numbers not present")
	}
	beds, err := strconv.ParseInt(bedsStr, 10, 32)
	if err != nil {
		return 0, err
	}
	return uint(beds), err
}

func parseBaths(input string) (uint, error) {
	regex := `^\s*(?P<Count>\d+)\s*(?P<Type>Baths|Bath\s*rooms)`
	trimmedStr := strings.TrimSpace(input)
	params := getParams(regex, trimmedStr)
	bathStr, ok := params["Count"]
	if !ok {
		return 0, fmt.Errorf("Bedrooms numbers not present")
	}
	baths, err := strconv.ParseInt(bathStr, 10, 32)
	if err != nil {
		return 0, err
	}
	return uint(baths), err
}

func parseSqft(input string) (uint, error) {
	regex := `^\s*(?P<Size>[\d,]+)\s*(?P<Type>([Ss]qft|Square\s*Feet))\s*$`
	trimmedStr := strings.TrimSpace(input)
	params := getParams(regex, trimmedStr)
	lotsize, ok := params["Size"]
	if !ok {
		return 0, fmt.Errorf("LotSize number not present")
	}
	re := regexp.MustCompile(",")
	lotsizewitnonlynumbers := re.ReplaceAllString(lotsize, "")
	sqft, err := strconv.ParseInt(lotsizewitnonlynumbers, 10, 32)
	if err != nil {
		return 0, err
	}
	return uint(sqft), err
}

func parseMls(input string) (string, error) {
	trimmedStr := strings.TrimSpace(input)
	params := getParams(`^\s*MLS/Source ID\s*:\s*(?P<ID>\d+)`, trimmedStr)

	mlsid, ok := params["ID"]
	if !ok {
		return "", fmt.Errorf("No MLS ID Present")
	}
	return mlsid, nil
}

func parseLotSize(input string) (uint, error) {
	regex := `^\s*Lot\s*Size\s*:\s*(?P<LotSize>(\d+\.\d*)|(\d+))\s*(?P<Type>sqft|acres)`
	var lotSizeData uint = 0

	trimmedStr := strings.TrimSpace(input)
	params := getParams(regex, trimmedStr)

	lotsize, ok := params["LotSize"]
	if !ok {
		return lotSizeData, fmt.Errorf("LotSize number not present")
	}

	lotSizeNumeric, err := strconv.ParseFloat(lotsize, 32)
	if err != nil {
		return lotSizeData, err
	}

	tmplotSize := float32(lotSizeNumeric)
	lottype, ok := params["Type"]
	if !ok {
		return lotSizeData, fmt.Errorf("Acreage/Sqft not present")
	}

	if strings.TrimSpace(lottype) == "acres" {
		lotSizeData = uint(tmplotSize * DataModels.SQFTPERACRE)
	}
	return lotSizeData, nil
}

func parseForPropType(input string) (string, error) {
	propType := DataModels.InvalidPropType
	resultStr := strings.TrimSpace(input)
	lowercasedStr := strings.ToLower(resultStr)
	switch lowercasedStr {
	case "condo":
		propType = DataModels.Condo
	case "single-family home":
		propType = DataModels.SingleFamilyHome
	case "multi-family":
		propType = DataModels.MultiFamily
	case "townhouse":
		propType = DataModels.TownHome
	case "land":
	case "lot/land":
		propType = DataModels.Land
	default:
		return propType, fmt.Errorf("Unable to match Property:%s", input)
	}
	//glog.Infof("PropertyType : %v ,%d",lowercasedStr,propType)
	return propType, nil
}

func parseDaysInMarket(input string) (uint, error) {
	//glog.Infof("Days Input Str:%v",input)
	regex := `^\s*(?P<Days>\d+)\s*days\s*on\s*Trulia`
	trimmedStr := strings.TrimSpace(input)
	params := getParams(regex, trimmedStr)
	daysInMarketStr, ok := params["Days"]
	if !ok {
		return 0, fmt.Errorf("Days in Market not present")
	}
	daysInMarket, err := strconv.ParseInt(daysInMarketStr, 10, 32)
	if err != nil {
		return 0, err
	}
	return uint(daysInMarket), err
}

func parseViewCount(input string) (uint, error) {
	regex := `^\s*(?P<Views>\d+)\s*[vV]iews\s*$`
	trimmedStr := strings.TrimSpace(input)
	params := getParams(regex, trimmedStr)
	viewCountStr, ok := params["Views"]
	if !ok {
		return 0, fmt.Errorf("View count is not preent in inputstr")
	}

	viewCount, err := strconv.ParseInt(viewCountStr, 10, 32)
	if err != nil {
		return 0, err
	}
	return uint(viewCount), err
}

func parseBuiltYear(input string) (uint, error) {
	regex := `^\s*[Bb]uilt\s*in\s*(?P<BuiltYear>\d+)\s*$`
	trimmedStr := strings.TrimSpace(input)
	params := getParams(regex, trimmedStr)
	builtYrStr, ok := params["BuiltYear"]
	if !ok {
		return 0, fmt.Errorf("Built Year is not preent in inputstr")
	}

	builtYear, err := strconv.ParseInt(builtYrStr, 10, 32)
	if err != nil {
		return 0, err
	}
	return uint(builtYear), err
}

func parseParkingSpaces(input string) (uint, error) {
	regex := `^\s*[Pp]arking\s*[Ss]paces\s*:\s*(?P<ParkingSpaces>\d+)\s*$`
	trimmedStr := strings.TrimSpace(input)
	params := getParams(regex, trimmedStr)

	parkingSpacesStr, ok := params["ParkingSpaces"]
	if !ok {
		return 0, fmt.Errorf("Parking Spaces is not preent in inputstr")
	}

	parkingSpaces, err := strconv.ParseInt(parkingSpacesStr, 10, 32)
	if err != nil {
		return 0, err
	}
	return uint(parkingSpaces), err
}

func getParams(regEx, url string) (paramsMap map[string]string) {
	var compRegEx = regexp.MustCompile(regEx)
	match := compRegEx.FindStringSubmatch(url)

	paramsMap = make(map[string]string)
	for i, name := range compRegEx.SubexpNames() {
		if i > 0 && i <= len(match) {
			paramsMap[name] = match[i]
		}
	}
	return
}
