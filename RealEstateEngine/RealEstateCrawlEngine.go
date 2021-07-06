package main

import (
	"flag"
	"fmt"
	"github.com/golang/glog"
	"os"
)

var options struct {
	config  string `validate:"nonzero"`
	jobmode bool   `validate:"nonzero"`
}

func init() {
	defaultUsage := flag.Usage
	flag.Usage = func() {
		fmt.Fprint(os.Stderr, "This service periodically generates Estate AI  ")
		defaultUsage()
	}

	flag.StringVar(&options.config, "config", "SampleConfigFile.yaml",
		"REQUIRED: Estate AI Configuration file")
	flag.BoolVar(&options.jobmode, "jobmode", false,
		"REQUIRED: Estate AI Configuration file")


	//flag.Set("logtostderr", "false")
	flag.Set("alsologtostderr", "true")
	flag.Parse()
}
func main() {
	defer glog.Flush()
	glog.Infof(" Estate Analytics Started .. ")
	glog.Infof(" Options for this instance of application  :%v", options)
	defer glog.Flush()

	glog.Infof("Hello RealEstate Engine");

}
