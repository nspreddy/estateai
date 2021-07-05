package PropertyAnalyzer

import (
	"fmt"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"nrideas.visualstudio.com/EstateAI/src/IpcUtils"
	"nrideas.visualstudio.com/EstateAI/src/MetricsAndStats"
)

func init() {

}

func StartPropertyAnalyzerEngine(analyzerConfig DataModels.PropertyAnalyzerConfiguration, apikeys DataModels.ApiKeysConfiguration) {

	//glog.Infof("Starting Property Analysis Engine Instance..")
	for {
		select {
		case propToAnalyze := <-IpcUtils.ChannelStore.PropertyAnalyzer:
			processProprtyRecord(&propToAnalyze)
			pegStats(&propToAnalyze)
			// provide updated Record to Data Uploader to upload to various sources for reports.
			IpcUtils.ChannelStore.DataUploaderChannel <- propToAnalyze
		}
	}
}

func processProprtyRecord(prop *DataModels.PropertyModel) {

	prop.WalkScore = 10
	if prop.Sqft > 0 && prop.PropertyType != DataModels.Land {
		prop.PricePerSqft = uint(prop.Price / prop.Sqft)
	}

	// ToDo : Walk Score
	// ToDo : Near By Check ( for universities , Park&Ride, Schools etc.. ) by property type
	// ToDo : Get Lat Long And Store in Record
	// ToDo:  Cross check with Historic data
	// ToDo:  Create ML Based Score
}

func pegStats(prop *DataModels.PropertyModel) {

	//1. Zip Code count
	MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,prop.ZipCode)
	//2. Zip Code by City
	MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,prop.City)
	//3. Count by Prop Type .. Single Family etc..
	MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,prop.PropertyType)

	// Price Range
	switch {
	case prop.Price <= 250000:
		MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,MetricsAndStats.LessThanQtr)
	case prop.Price > 250000 && prop.Price <= 500000:
		MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,MetricsAndStats.QtrToHalf)
	case prop.Price > 500000 && prop.Price <= 1000000:
		MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,MetricsAndStats.HalfToMil)
	case prop.Price > 1000000 && prop.Price <= 2000000:
		MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,MetricsAndStats.MilToTwoMil)
	case prop.Price > 2000000:
		MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,MetricsAndStats.AboveTwoMil)
	}

	// Year Built on Sale
	switch {
	case prop.BuiltYear <= 1990:
		MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,MetricsAndStats.Before1990)
	case prop.BuiltYear > 1990 && prop.BuiltYear < 2000:
		MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,MetricsAndStats.After1990Before2000)
	case prop.BuiltYear > 2000 && prop.BuiltYear < 2010:
		MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,MetricsAndStats.After2000Before2010)
	case prop.BuiltYear > 2010 && prop.BuiltYear < 2017:
		MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,MetricsAndStats.After2010)
	}

	// Beds And Bath Count
	switch prop.PropertyType {
	case DataModels.SingleFamilyHome, DataModels.TownHome, DataModels.Condo:
		bedbathkey := fmt.Sprintf("Bed-%d_Bath-%d", prop.Beds, prop.Baths)
		MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,bedbathkey)
	case DataModels.Land:
		lotsizeinAcers := prop.LotSize / DataModels.SQFTPERACRE

		switch {
		case lotsizeinAcers <= 1.0:
			MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,MetricsAndStats.LessThan1Acre)
		case lotsizeinAcers > 1.0 && lotsizeinAcers <= 5.0:
			MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,MetricsAndStats.LessThan5AcreMoreThan1)
		case lotsizeinAcers > 5.0 && lotsizeinAcers <= 10.0:
			MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,MetricsAndStats.LessThan10AcreMorethan1)
		case lotsizeinAcers > 10.0 && lotsizeinAcers <= 20.0:
			MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,MetricsAndStats.LessThan20AcreMorethan10)
		case lotsizeinAcers > 20.0 && lotsizeinAcers <= 50.0:
			MetricsAndStats.IncrementJobMetric(prop.CrawlJobId,MetricsAndStats.Morethan20)
		}
	}

}
