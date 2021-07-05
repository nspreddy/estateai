package ZooCode_Experiments

import (
	"fmt"
	"golang.org/x/net/context"
	"golang.org/x/oauth2/google"
	"google.golang.org/api/sheets/v4"
	"io/ioutil"
	"testing"
)

func TestSheetApiUtil(t *testing.T) {

	ctx := context.Background()

	b, err := ioutil.ReadFile("client_secret.json")
	if err != nil {
		fmt.Printf("Unable to read client secret file: %v", err)
	}

	// If modifying these scopes, delete your previously saved credentials
	// at ~/.credentials/sheets.googleapis.com-go-quickstart.json
	config, err := google.ConfigFromJSON(b, "https://www.googleapis.com/auth/spreadsheets")
	if err != nil {
		fmt.Printf("Unable to parse client secret file to config: %v", err)
	}
	client := getClient(ctx, config)

	srv, err := sheets.New(client)
	if err != nil {
		fmt.Printf("Unable to retrieve Sheets Client %v", err)
	}

	/*
		// Prints the names and majors of students in a sample spreadsheet:
		// https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
		spreadsheetId := "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms"
		//spreadsheetId := "1_SBuEN9-NaUCqYpjAVLH6APvLn_sh1tBML_YtumCLSc"
		readRange := "Class Data!A2:E"
		resp, err := srv.Spreadsheets.Values.Get(spreadsheetId,readRange).Do()
		if err != nil {
			fmt.Printf("Unable to retrieve data from sheet. %v", err)
			return
		}

		if len(resp.Values) > 0 {
			fmt.Println("Name, Major:")
			for _, row := range resp.Values {
				// Print columns A and E, which correspond to indices 0 and 4.
				fmt.Printf("%s, %s\n", row[0], row[1])
			}
		} else {
			fmt.Print("No data found.")
		}
	*/

	spreadsheetId := "1_SBuEN9-NaUCqYpjAVLH6APvLn_sh1tBML_YtumCLSc"
	readRange := "Sheet1!A:C"
	resp, err := srv.Spreadsheets.Values.Get(spreadsheetId, readRange).Do()
	if err != nil {
		fmt.Printf("Unable to retrieve data from sheet. %v", err)
		return
	}

	if len(resp.Values) > 0 {
		fmt.Println("Record :")
		for _, row := range resp.Values {
			// Print columns A and E, which correspond to indices 0 and 4.
			fmt.Printf("%s, %s %s\n", row[0], row[1], row[2])
		}
	} else {
		fmt.Print("No data found.")
	}

}

func TestSheetApiB2BTest(t *testing.T) { // Your credentials should be obtained from the Google

	data, err := ioutil.ReadFile("ClientB2B.json")
	if err != nil {
		fmt.Print(err)
	}
	conf, err := google.JWTConfigFromJSON(data, "https://www.googleapis.com/auth/spreadsheets.readonly")
	if err != nil {
		fmt.Print(err)
	}

	client := getClientBasedonJwtConfig(context.TODO(), conf)

	srv, err := sheets.New(client)
	if err != nil {
		fmt.Printf("Unable to retrieve Sheets Client %v", err)
	}

	// Prints the names and majors of students in a sample spreadsheet:
	// https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
	// https://docs.google.com/spreadsheets/d/1_SBuEN9-NaUCqYpjAVLH6APvLn_sh1tBML_YtumCLSc/edit#gid=0
	spreadsheetId := "1_SBuEN9-NaUCqYpjAVLH6APvLn_sh1tBML_YtumCLSc"
	readRange := "Sheet1!A:C"
	resp, err := srv.Spreadsheets.Values.Get(spreadsheetId, readRange).Do()
	if err != nil {
		fmt.Printf("Unable to retrieve data from sheet. %v", err)
		return
	}

	if len(resp.Values) > 0 {
		fmt.Println("Record :")
		for _, row := range resp.Values {
			// Print columns A and E, which correspond to indices 0 and 4.
			fmt.Printf("%s, %s %s\n", row[0], row[1], row[2])
		}
	} else {
		fmt.Print("No data found.")
	}

}
