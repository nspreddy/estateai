package Utils

import (
	"fmt"
	"google.golang.org/api/sheets/v4"
	"testing"
)

var clientSecretfilename = "client_secret.json"
var scope = "https://www.googleapis.com/auth/spreadsheets"
var code = "4/eWk7jlW4ypvj_4UOZx8chW5wSrSdQ82oX8y4MyZUn90"

func TestSheetApiUtil(t *testing.T) {

	client, err := getGoogleApiClient(clientSecretfilename, code, scope)
	if err != nil {
		fmt.Printf("Unable to retrieve Google Client %v", err)
	}

	srv, err := sheets.New(client)
	if err != nil {
		fmt.Printf("Unable to retrieve Sheets Client %v", err)
	}

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

func TestGenerateUrl(t *testing.T) {

	url, err := getGoogleApiAuthUrl(clientSecretfilename, code, scope)
	if err != nil {
		fmt.Printf("Unable to retrieve Google Client %v", err)
	}

	fmt.Printf("please click this URL to get code to configure :%v", url)

}
