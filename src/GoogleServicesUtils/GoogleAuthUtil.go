package Utils

import (
	"encoding/json"
	"fmt"
	"golang.org/x/net/context"
	"golang.org/x/oauth2"
	"golang.org/x/oauth2/google"
	"golang.org/x/oauth2/jwt"
	"io/ioutil"
	"log"
	"net/http"
	"net/url"
	"os"
	"os/user"
	"path/filepath"
)

func getClientBasedonJwtConfig(ctx context.Context, config *jwt.Config) *http.Client {
	client := config.Client(ctx)
	return client
}

// Code :
// Scope (ex: "https://www.googleapis.com/auth/spreadsheets")

func getGoogleApiClient(clientsecretfile string, code string, scope string) (*http.Client, error) {
	ctx := context.Background()
	b, err := ioutil.ReadFile(clientsecretfile)
	if err != nil {
		return nil, fmt.Errorf("Unable to read client secret file: %v", err)
	}
	config, err := google.ConfigFromJSON(b, scope)
	if err != nil {
		return nil, fmt.Errorf("Unable to parse client secret file to config: %v", err)
	}
	client, err := getClient(ctx, config, code)
	return client, err
}

func getGoogleApiAuthUrl(clientsecretfile string, code string, scope string) (string, error) {
	b, err := ioutil.ReadFile(clientsecretfile)
	if err != nil {
		return "", fmt.Errorf("Unable to read client secret file: %v", err)
	}
	config, err := google.ConfigFromJSON(b, scope)
	if err != nil {
		return "", fmt.Errorf("Unable to parse client secret file to config: %v", err)
	}
	return getAuthUrlFromWeb(config), nil
}

// getClient uses a Context and Config to retrieve a Token
// then generate a Client. It returns the generated Client.
func getClient(ctx context.Context, config *oauth2.Config, code string) (*http.Client, error) {
	cacheFile, err := tokenCacheFile()
	if err != nil {
		return nil, fmt.Errorf("Unable to get path to cached credential file. %v", err)
	}
	tok, err := tokenFromFile(cacheFile)
	if err != nil {
		tok, err = getTokenFromCode(config, code)
		if err != nil {
			return nil, fmt.Errorf("Unable to generate Token , err:%v", err)
		}
		saveToken(cacheFile, tok)
	}
	return config.Client(ctx, tok), nil
}

func getTokenFromCode(config *oauth2.Config, code string) (*oauth2.Token, error) {
	tok, err := config.Exchange(oauth2.NoContext, code)
	if err != nil {
		return nil, fmt.Errorf("Unable to get Token from google API End point, Error:%v", err)
	}
	return tok, nil
}

// getTokenFromWeb uses Config to request a Token.
// It returns the retrieved Token.
func getAuthUrlFromWeb(config *oauth2.Config) string {
	authURL := config.AuthCodeURL("state-token", oauth2.AccessTypeOffline)
	return authURL
}

// tokenCacheFile generates credential file path/filename.
// It returns the generated credential path/filename.
func tokenCacheFile() (string, error) {
	usr, err := user.Current()
	if err != nil {
		return "", err
	}
	tokenCacheDir := filepath.Join(usr.HomeDir, ".credentials")
	os.MkdirAll(tokenCacheDir, 0700)
	return filepath.Join(tokenCacheDir,
		url.QueryEscape("ReCrawlerGoogleToken.json")), err
}

// tokenFromFile retrieves a Token from a given file path.
func tokenFromFile(file string) (*oauth2.Token, error) {
	f, err := os.Open(file)
	if err != nil {
		return nil, err
	}
	t := &oauth2.Token{}
	err = json.NewDecoder(f).Decode(t)
	defer f.Close()
	return t, err
}

// saveToken uses a file path to create a file and store the
// token in it.
func saveToken(file string, token *oauth2.Token) {
	fmt.Printf("Saving credential file to: %s\n", file)
	f, err := os.OpenFile(file, os.O_RDWR|os.O_CREATE|os.O_TRUNC, 0600)
	if err != nil {
		log.Fatalf("Unable to cache oauth token: %v", err)
	}
	defer f.Close()
	json.NewEncoder(f).Encode(token)
}
