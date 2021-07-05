// Copyright 2015 Google Inc. All rights reserved.
// Use of this source code is governed by the Apache 2.0
// license that can be found in the LICENSE file.

// Sample helloworld is a basic App Engine flexible app.
package main

import (
	"fmt"
	"log"
	"net/http"
    "time"
)

func main() {
	http.HandleFunc("/", handle)
	http.HandleFunc("/_ah/health", healthCheckHandler)
	log.Println("Listening on port 8080")
    go LoopForNSeconds()
	log.Fatal(http.ListenAndServe(":8080", nil))


}

func LoopForNSeconds(){

	// make sure StatsTable is created
	sleepTicker := time.NewTicker(1 * time.Second).C
	for range sleepTicker {
      log.Println("Hello World , You rock !!");
	}
}

func handle(w http.ResponseWriter, r *http.Request) {
	if r.URL.Path != "/" {
		http.NotFound(w, r)
		return
	}
	fmt.Fprint(w, "Hello world!")
}

func healthCheckHandler(w http.ResponseWriter, r *http.Request) {
	fmt.Fprint(w, "ok")
}
