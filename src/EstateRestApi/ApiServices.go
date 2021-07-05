package EstateRestApi

import (
	"encoding/json"
	"fmt"
	"github.com/golang/glog"
	"github.com/gorilla/mux"
	"net/http"
	"nrideas.visualstudio.com/EstateAI/src/DataModels"
	"nrideas.visualstudio.com/EstateAI/src/MetricsAndStats"
)

func init() {

}

func StartRestApiService(commonModuleConfig DataModels.CommonConfigForModules, apiConfig DataModels.ApiServiceConfiguration) {
	glog.Infof("Common Module Configuration: %v", commonModuleConfig)
	// Load Configuration from config file.
	listenPort := fmt.Sprintf(":%v", "8080")

	// Read Configuration and start crawling ..

	router := mux.NewRouter().StrictSlash(true)
	router.HandleFunc("/", Index)
	router.HandleFunc("/api", RestApiHandler)
	router.HandleFunc("/healthChk", HealthCheck)
	router.HandleFunc("/metrics", MetricsHandler)
	glog.Infof("Listening @%v", listenPort)
	glog.Fatal(http.ListenAndServe(listenPort, router))
}

func Index(w http.ResponseWriter, r *http.Request) {
	fmt.Fprintln(w, "Welcome!")
}

func RestApiHandler(w http.ResponseWriter, r *http.Request) {
	mockapiresult := DataModels.MockApiModel{"Sample", true}
	json.NewEncoder(w).Encode(mockapiresult)
}

func HealthCheck(w http.ResponseWriter, r *http.Request) {
	hlthResult := DataModels.HealthCheck{true, nil}
	json.NewEncoder(w).Encode(hlthResult)
}

func MetricsHandler(w http.ResponseWriter, r *http.Request) {
	// Print Metrics
	MetricsAndStats.WriteStatsToEncoder(json.NewEncoder(w))
}
