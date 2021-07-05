package ConfigurationUtils

import (
	"fmt"
	"github.com/ghodss/yaml"
	"io/ioutil"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
)

func LoadConfiguration(configfile string) (DataModels.Config, error) {
	cfg := DataModels.Config{}
	yamlFile, err := ioutil.ReadFile(configfile)
	if err != nil {
		return cfg, fmt.Errorf("Failed to read YAML File :%v, Error:%v ", configfile, err)
	}

	err = yaml.Unmarshal(yamlFile, &cfg)
	if err != nil {
		return cfg, fmt.Errorf("Failed to parse configuration (Unmarshal error) :%v  Error: %v", string(yamlFile), err)
	}
	//glog.Infof(" Raw YAML Contents :%v from filename: %v", string(yamlFile), configfile)
	return cfg, nil
}
