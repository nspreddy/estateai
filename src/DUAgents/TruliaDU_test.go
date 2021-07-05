package DUAgents

import (
	"fmt"
	"regexp"
	"testing"
)

func TestParsingBeds(t *testing.T) {

	teststr1 := "3 Beds"
	testStr2 := "5 Bedrooms"

	result1, err := parseBeds(teststr1)
	fmt.Printf("beds :%d , err: %s\n", result1, err)

	result2, err := parseBeds(testStr2)
	fmt.Printf("beds :%d , err:%s\n", result2, err)
}

func TestParsingBaths(t *testing.T) {

	teststr1 := "3 Baths"
	testStr2 := "7 Bath   rooms"

	result1, err := parseBaths(teststr1)
	fmt.Printf("Baths :%d , err: %s\n", result1, err)

	result2, err := parseBaths(testStr2)
	fmt.Printf("Baths :%d , err:%s\n", result2, err)
}

func TestParsingSqft(t *testing.T) {
	testArray := []string{"3,000 sqft", "500 Square     Feet", "Lot Size: 5227 sqft", "8346 sqft lot size"}

	for _, testStr := range testArray {

		sqft, err := parseSqft(testStr)
		if err == nil {
			fmt.Printf(" Sqft  :%d for teststr :%s\n", sqft, testStr)
		} else {
			fmt.Printf("Error:%v\n", err)
		}
		/*
			regex:=`\s*(?P<Size>[\d,]+)\s*(?P<Type>[Ss]qft|Square\s*Feet)\s*$`
			params := getParams(regex, testStr)
			fmt.Println(params)*/
	}

}

func TestMLSId(t *testing.T) {
	testmlsStr := "MLS/Source ID: 1093016"

	mlsid, err := parseMls(testmlsStr)
	if err == nil {
		fmt.Printf("MLS :%s\n", mlsid)
	} else {
		fmt.Printf("Err:%v", err)
	}
}

func TestLotSizeAcreOrSQft(t *testing.T) {
	testmlsStr := " Lot Size: 5227 sqft"
	testmlsStr2 := "Lot Size: 4.94 acres"

	lotData, err := parseLotSize(testmlsStr)
	if err == nil {
		fmt.Printf("Lot Sqft :%v\n", lotData)
	} else {
		fmt.Printf("Err:%v", err)
	}

	lotData, err = parseLotSize(testmlsStr2)
	if err == nil {
		fmt.Printf("Lot Size(Converted from Acers) :%v\n", lotData)
	} else {
		fmt.Printf("Err:%v", err)
	}

	regex := `^\s*Lot\s*Size\s*:\s*(?P<LotSize>(\d+\.\d*)|(\d+))\s*(?P<Type>sqft|acres)`
	params := getParams(regex, testmlsStr)
	fmt.Println(params)

	params = getParams(regex, testmlsStr2)
	fmt.Println(params)

}

func TestCityTest(t *testing.T) {
	regex := `\s*(?P<City>\D+)\s*,\s*(?P<State>\D+)\s*(?P<ZipCode>\d+)`
	sampleStr := "Sammamish, WA 98075"

	params := getParams(regex, sampleStr)
	fmt.Println(params)

}

func TestDaysInMarket(t *testing.T) {
	testArray := []string{"82 days on Trulia", "75 days in Trulia", "Super 82 days on Trulia "}

	for _, testStr := range testArray {
		days, err := parseDaysInMarket(testStr)
		if err == nil {
			fmt.Printf(" Days in Marker is :%d for teststr :%s\n", days, testStr)
		} else {
			fmt.Printf("Error:%v\n", err)
		}

		/*params := getParams(`^\s*(?P<Days>\d+)\s*\s*days\s*on\s*Trulia`, testStr)
		fmt.Println(params)*/
	}
}

func TestViews(t *testing.T) {
	testArray := []string{"243 views", "675 Views Super", "Super 876 views"}

	for _, testStr := range testArray {

		views, err := parseViewCount(testStr)
		if err == nil {
			fmt.Printf(" Views  :%d for teststr :%s\n", views, testStr)
		} else {
			fmt.Printf("Error:%v\n", err)
		}
		/*
			params := getParams(`^\s*(?P<Views>\d+)\s*[vV]iews\s*$`, testStr)
			fmt.Println(params)*/
	}
}

func TestBuiltYear(t *testing.T) {
	testArray := []string{"Built in 1981", "Super cool Built in 1971", "Built in 1981 Cool Stuff"}

	for _, testStr := range testArray {

		yrBuilt, err := parseBuiltYear(testStr)
		if err == nil {
			fmt.Printf(" Year Built:%d for teststr :%s\n", yrBuilt, testStr)
		} else {
			fmt.Printf("Error:%v\n", err)
		}
		/*
			params := getParams(`^\s*[Bb]uilt\s*in\s*(?P<BuiltYear>\d+)\s*$`, testStr)
			fmt.Println(params)*/
	}
}

func TestParkingSpots(t *testing.T) {
	testArray := []string{"Parking Spaces: 3", "Super Parking Spaces: 3", "Parking Spaces: 3 Stuff"}

	for _, testStr := range testArray {

		parkingSpaces, err := parseParkingSpaces(testStr)
		if err == nil {
			fmt.Printf("Parking Spaces:%d for teststr :%s\n", parkingSpaces, testStr)
		} else {
			fmt.Printf("Error:%v\n", err)
		}
		/*
			params := getParams(`^\s*[Pp]arking\s*[Ss]paces\s*:\s*(?P<ParkingSpaces>\d+)\s*$`, testStr)
			fmt.Println(params)*/
	}
}

func TestSampleRegEx(t *testing.T) {
	r := regexp.MustCompile(`(?P<Year>\d{4})-(?P<Month>\d{2})-(?P<Day>\d{2})`)
	fmt.Printf("%#v\n", r.FindStringSubmatch(`2015-05-27`))

	params := getParams(`(?P<Year>\d{4})-(?P<Month>\d{2})-(?P<Day>\d{2})`, `2015-05-27`)
	fmt.Println(params)

}
