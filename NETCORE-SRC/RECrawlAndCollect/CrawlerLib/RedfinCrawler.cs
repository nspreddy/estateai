using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonAndUtils;
using DataModels;
using HtmlAgilityPack;


namespace CrawlerLib
{
    class RedfinCrawler: CrawlerBase
    {
        #region PRIVATE_MEMEBRS

        #endregion

        #region PRIVATE_METHODS

        enum URLType { PROPERTY,COUNTY_STATS,CITY_STATS,ZIPCODE_STATS,NGHOOD_STATS, XMLPROPTYLIST, UNKNOWN};
        private const string SITEMAPXML = "sitemap-xml/";
        private URLType getUrlType( Uri uri)
        {
            URLType typeOfUrl = URLType.UNKNOWN;

            switch( uri.Segments.Length)
            {
                case 6:
                    // https://www.redfin.com/WA/Moses-Lake/7883-Dune-Lake-Rd-SE-98837/home/16825240 Property URL
                    typeOfUrl = URLType.PROPERTY;
                    break;
                case 3:
                     // https://www.redfin.com/sitemap-xml/listings_WA_sitemap.xml.gz
                     if( uri.Segments[1].Trim() == SITEMAPXML)
                    {
                        typeOfUrl = URLType.XMLPROPTYLIST;
                    }
                    break;
            }


            return typeOfUrl;
        }

        private const string XPATH_PROP_ADDRESS = "//div[@class='street-address']";
        private const string ADDRESS_TITLE = "title";
        private bool extractAddress(HtmlDocument htmlDoc, Property prop)
        {
            try
            {
                var addressNode = htmlDoc.DocumentNode.SelectSingleNode(XPATH_PROP_ADDRESS);
                if (addressNode != null)
                {
                    var address = addressNode.Attributes[ADDRESS_TITLE]?.Value;
                    prop.ProcessHouseAndStreet(address);
                    Console.WriteLine($"Address extracted is {address}");
                }
                return true;

            }
            catch(Exception e)
            {
                Console.WriteLine($"Exception while processing doc for address {e.Message}");
            }
            return false;
        }

        private const string XPATH_CITY_STATE_ADDRESS = "//div[@data-rf-test-id='abp-cityStateZip']";
        private bool extractCityStateZip(HtmlDocument htmlDoc, Property prop)
        {
            try
            {
                var cityStateZipNode = htmlDoc.DocumentNode.SelectSingleNode(XPATH_CITY_STATE_ADDRESS);
                if (cityStateZipNode != null)
                {
                    var cityStateZip = cityStateZipNode.InnerText.Trim();
                    prop.ProcessCityStateZip(cityStateZip);
                    Console.WriteLine($"CityStateZip extracted is {cityStateZip}");
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while processing doc for address {e.Message}");
            }
            return false;
        }



        private void CrawlAndExtractPropertyDetails( string url)
        {
            try
            {
                HtmlDocument htmlDoc = CrawlUtils.getHtmlDocFromUrl(url);
                // Extract Street Address
                if (htmlDoc != null)
                {
                    Property property = new Property();

                    if( !extractAddress(htmlDoc, property))
                    {
                        return;
                    }

                    if( !extractCityStateZip(htmlDoc, property))
                    {
                        return;
                    }
                    
                    //Console.WriteLine($"Crawled proprty URL: {url}");
                    MarkAsCrawled(url);
                }
                else
                {
                    Console.WriteLine($" Unbale to get HTML Doc .. may be throttled {url}");
                }
                
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception:  '{ex.Message}'");
            }
        }

        private const string XPATH_PROP_LINKS = "//loc";
        private void CrawlAndExtractPropertyLinks(string url)
        {
            try
            {
                HtmlDocument xmlDoc = CrawlUtils.getHtmlDocFromUrl(url);

                // This is list of XML list 
                if (xmlDoc != null)
                {
                    var propertyUrlNodes  = xmlDoc.DocumentNode.SelectNodes(XPATH_PROP_LINKS);

                    if (propertyUrlNodes != null)
                    {
                        List<string> propertyLinks = new List<string>();

                        foreach (var propNode in propertyUrlNodes)
                        {
                            var propLinkUrl = propNode.InnerText;
                            Console.WriteLine($"Prop-Link: {propLinkUrl}");
                            propertyLinks.Add(propLinkUrl);
                        }
                        Console.WriteLine($"Total Prop Links: {propertyLinks.Count}");
                        MarkAsCrawled(url);
                    }
                }
                else
                {
                    Console.WriteLine($" Unbale to get HTML Doc .. may be throttled {url}");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception:  '{ex.Message}'");
            }
        }


        #endregion
        protected override bool ProcessURI(Uri uri)
        {
            switch(getUrlType(uri))
            {
                case URLType.PROPERTY:
                    CrawlAndExtractPropertyDetails(uri.AbsoluteUri);
                    break;
                case URLType.XMLPROPTYLIST:
                    CrawlAndExtractPropertyLinks(uri.AbsoluteUri);
                    break;
            }
            return true;
        }
    }
}
