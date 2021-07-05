package AreaFeeder

import (
	"fmt"
	"github.com/ghodss/yaml"
	"github.com/golang/glog"
	"io/ioutil"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"time"
)

const (
	defaultConfigFile  = "areaconfig.json"
	defaultCrawlPeriod = 120 // 60 mins
)

type AreaFeed map[string]DataModels.AreaDefinition

func init() {

}

func StartAreaFeeder(commonModuleConfig DataModels.CommonConfigForModules, areaCfg DataModels.AreaFeederConfiguration, areaInputCh chan DataModels.AreaDefinition) {

	glog.Infof("Common Module Configuration: %v", commonModuleConfig)

	configfile := defaultConfigFile
	crawlPeriod := defaultCrawlPeriod
	if len(areaCfg.AreaFeedConfigFile) > 0 {
		configfile = areaCfg.AreaFeedConfigFile
	}

	if areaCfg.CrawlInterval > 0 {
		crawlPeriod = areaCfg.CrawlInterval
	}

	computedDuration := time.Duration(crawlPeriod*60) * time.Second
	glog.Infof("Computed Duration for Ticker(in seconds):%v", computedDuration)
	crawlTicker := time.NewTicker(computedDuration).C

	for {
		err := readFileandQueueCrawltasks(configfile, areaInputCh)
		if err != nil {
			glog.Errorf(" Error processing crawl requests: %v", err)
		}
		<-crawlTicker
	}

}

func readFileandQueueCrawltasks(configfile string, areaInputCh chan DataModels.AreaDefinition) error {
	areaFeedConfiguration := AreaFeed{}
	yamlFile, err := ioutil.ReadFile(configfile)
	if err != nil {
		return fmt.Errorf("Failed to read YAML File :%v, Error:%v ", configfile, err)

	}

	err = yaml.Unmarshal(yamlFile, &areaFeedConfiguration)
	if err != nil {
		return fmt.Errorf("Failed to parse configuration (Unmarshal error) :%v  Error: %v", string(yamlFile), err)
	}
	glog.Infof("Number of Areas defined in configuration:%d, %v", len(areaFeedConfiguration), configfile)

	// Iterate thry Hash Map
	for areaname, areaDef := range areaFeedConfiguration {
		glog.Infof(" Queueing the Area definition: %v:%v", areaname, areaDef)
		areaDef.Name = areaname
		areaInputCh <- areaDef
	}
	return nil
}
