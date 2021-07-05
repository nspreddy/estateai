package ReWebCrawl

import (
	"fmt"
	"github.com/PuerkitoBio/goquery"
	"github.com/golang/glog"
	"golang.org/x/net/html"
	"strings"
	"testing"
)

func TestParsingHtml(t *testing.T) {

	s := `<p>Links:</p><ul><li><a href="foo">Foo</a><li><a href="/bar/baz">BarBaz</a></ul>`
	doc, err := html.Parse(strings.NewReader(s))
	if err != nil {
		glog.Fatal(err)
	}
	var f func(*html.Node)
	f = func(n *html.Node) {
		if n.Type == html.ElementNode && n.Data == "a" {
			glog.Infof("Data:%v", n)
			for _, a := range n.Attr {
				glog.Infof("key: %v, Value:%v", a.Key, a.Val)
				if a.Key == "href" {
					glog.Infof("Value :%v", a.Val)
					break
				}
			}
		}
		for c := n.FirstChild; c != nil; c = c.NextSibling {
			f(c)
		}
	}
	f(doc)

}

func TestParsingHtmlWithGoQuery(t *testing.T) {

	s := `<p>Links:</p><ul><li><a href="foo">Foo</a><li><a href="/bar/baz">BarBaz</a></ul><a href="boo">Boo</a>`
	htmlDoc, err := html.Parse(strings.NewReader(s))
	if err != nil {
		glog.Fatal(err)
	}

	doc := goquery.NewDocumentFromNode(htmlDoc)
	if err != nil {
		glog.Fatal(err)
	}

	// Find the review items
	doc.Find("a").Each(func(i int, s *goquery.Selection) {
		// For each item found, get the band and title
		link, ok := s.Attr("href")
		if ok {
			fmt.Printf("HRef :%v\n", link)
		}

		text := s.Text()
		fmt.Printf("Text :%v\n", text)

		fmt.Printf("SelectedNode :%d\n", i)
	})

}
