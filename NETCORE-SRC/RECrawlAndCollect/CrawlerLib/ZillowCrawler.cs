using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using CommonAndUtils;
using System.Collections.Concurrent;
using System.Net;
using System.IO;

namespace CrawlerLib
{
    class ZillowCrawler
    {
        private const string ROOT = "/browse/homes/";
        private const string NEWHOMES_REGEX = "/browse/homes/([-a - zA - Z0 - 9@:% _\\+.~#?&=]*)/([-a-zA-Z0-9@:%_\\+.~#?&=]*)/newest-homes/";
        private const string PROP_PAGE_PREFIX = "homedetails/";
        private const string BROWSE = "browse/";
        private const string HOMES = "homes/";
        private const string NEW_SALE_HOMES = "newest-homes/";

        private ConcurrentQueue<Uri> urlQueue = new ConcurrentQueue<Uri>();
        private ConcurrentDictionary<string,bool> crawledUrlsDictionary = new ConcurrentDictionary<string, bool>();
        #region PRIVATE_METHODS
        private void AddUrlToTheQueue(string url)
        {
            if( !crawledUrlsDictionary.ContainsKey(url))
            {
                // Let us add to the queue
                urlQueue.Enqueue( new Uri(url));
            }
            else
            {
                Console.WriteLine($"Why we are adding already Crawled URL {url}");
            }
        }

        private void MarkAsCrawled(string url)
        {
            if (!crawledUrlsDictionary.ContainsKey(url))
            {
                crawledUrlsDictionary[url] = true;
            }
            else
            {
                Console.WriteLine($"Why we are making as crawled for  already Crawled URL {url}");
            }
        }

        private enum ContentHoldingByURI { PROPERTY, STATES, COUNTY, COUNTYZIPCODE,COUNTYSALEHOMES,ZIPCODEEXT, ZIPCODEEXTPROPS,MISC };

        private ContentHoldingByURI getTypeOfURL(string url)
        {
            try
            {
                var uri = new Uri(url);
                return getTypeOfURL(uri);
            }
            catch( Exception)
            {
                Console.WriteLine($"Could be a wrong URI Format: {url}");
                return ContentHoldingByURI.MISC;
            }
        }
        // get the type of URI.
        private ContentHoldingByURI getTypeOfURL(Uri inputUri)
        {
            ContentHoldingByURI returnValue = ContentHoldingByURI.MISC;

            var numberofSegments = inputUri.Segments.Length;
            switch (inputUri.Segments[1])
            {
                case PROP_PAGE_PREFIX:
                    // this is property link, let us crawl it diffrently.
                    returnValue = ContentHoldingByURI.PROPERTY;
                    break;
                case BROWSE:                    
                    if (inputUri.Segments[2] == HOMES)
                    {
                        switch (numberofSegments)
                        {
                            case 3:
                                // https://www.zillow.com/browse/homes/
                                // Crawl for States
                                returnValue = ContentHoldingByURI.STATES;                                
                                break;
                            case 4:
                                // https://www.zillow.com/browse/homes/wa/ 
                                // inca this case it will more of getting county links
                                returnValue = ContentHoldingByURI.COUNTY;
                                break;
                            case 5:
                                // https://www.zillow.com/browse/homes/wa/king-county/
                                // Let us crawl Zip Code pages 
                                returnValue =  ContentHoldingByURI.COUNTYZIPCODE;
                                break;
                            case 6:
                                //https://www.zillow.com/browse/homes/wa/king-county/newest-homes/
                                if (inputUri.Segments[5] == NEW_SALE_HOMES)
                                {
                                    returnValue = ContentHoldingByURI.COUNTYSALEHOMES;
                                    // Let us crawl links sale properties
                                }
                                else
                                {
                                    // https://www.zillow.com/browse/homes/wa/king-county/98052/ 
                                    // Let us crawl Zip Code pages( street level)
                                    returnValue = ContentHoldingByURI.ZIPCODEEXT;
                                }
                                break;
                            case 7:
                                //https://www.zillow.com/browse/homes/wa/king-county/98052/12/
                                returnValue = ContentHoldingByURI.ZIPCODEEXTPROPS;
                                break;

                        }
                    }
                    break;
            }
            return returnValue;

        }
               

        // this method cralws out put HTML of https://www.zillow.com/browse/homes/
        private const string XPATH_STATES_USA         = "//h2[.='United States']";
        private const string XPATH_STATES_CANADA      = "//h2[.='Canada']";
        private const string XPATH_STATES_ANCHORLINKS = "../ul[@class='bh-body-links']/li/a";
        private void ExtractAndQueueStateUrls( string url)
        {
            try
            {
                HtmlDocument htmlDoc = CrawlUtils.getHtmlDocFromUrl(url);
                // Let us test for  US states only for now.. 
                var countryNode = htmlDoc.DocumentNode.SelectSingleNode(XPATH_STATES_USA);

                if (countryNode != null)
                {
                    Console.WriteLine($" Country : {countryNode.InnerText} ");
                    //var stateNodes = htmlDoc.DocumentNode.SelectNodes("//ul[@class='bh-body-links']/li/a");
                    //var stateNodes = usStateH2Node.ParentNode.SelectNodes("./ul[@class='bh-body-links']/li/a");
                    var stateNodes = countryNode.SelectNodes(XPATH_STATES_ANCHORLINKS);

                    if (stateNodes == null)
                    {
                        Console.WriteLine($"Unable to get State URLs from {url}");
                        var node = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
                        Console.WriteLine("Node Name: " + node.Name + "\n" + node.OuterHtml);
                    }
                    else
                    {
                        foreach (var node in stateNodes)
                        {
                            //Console.WriteLine(" SubNode Name: " + node.Name + "\n" + node.InnerHtml + " Outter"+ node.OuterHtml);
                            var stateUrl = UrlUtils.GetAbsoluteUrlString(url, node.Attributes["href"].Value);
                            var stateName = node.InnerHtml;
                            switch (getTypeOfURL(stateUrl))
                            {
                                case ContentHoldingByURI.COUNTY:
                                    AddUrlToTheQueue(stateUrl);
                                    break;
                                default:
                                    break;
                            }
                            //Console.WriteLine($" State: '{stateName}', URL: '{stateUrl}'");
                            AddUrlToTheQueue(stateUrl);
                        }
                        // Let us mark this url as crawled.
                        MarkAsCrawled(url);
                    }
                }
                else
                {
                    Console.WriteLine(" Unable to parse due to Bot detect by Zillow");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception:  '{ex.Message}'");
            }
        }

        // this method cralws out put HTML of https://www.zillow.com/browse/homes/wa/
        private const string XPATH_UL_LI_ANCHOR_LINKS = "//ul/li/a";
        private void ExtractAndQueueCountyUrls(string url)
        {
            try
            {
                HtmlDocument htmlDoc = CrawlUtils.getHtmlDocFromUrl(url);

                // Let us test for  US states only for now.. 
                var countyNodes = htmlDoc.DocumentNode.SelectNodes(XPATH_UL_LI_ANCHOR_LINKS);
                if (countyNodes == null)
                {
                    Console.WriteLine($"Unable to get needed County Anchors from url: {url}");
                    var node = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
                    Console.WriteLine("Node Name: " + node.Name + "\n" + node.OuterHtml);
                }
                else
                {
                    foreach (var node in countyNodes)
                    {
                        //Console.WriteLine(" SubNode Name: " + node.Name + "\n" + node.InnerHtml + " Outter"+ node.OuterHtml);
                        var countyUrl = UrlUtils.GetAbsoluteUrlString(url, node.Attributes["href"].Value);

                        switch (getTypeOfURL(countyUrl))
                        {
                            case ContentHoldingByURI.COUNTYSALEHOMES:
                                AddUrlToTheQueue(countyUrl);
                                break;
                            case ContentHoldingByURI.COUNTYZIPCODE:
                                break;
                            default:
                                break;
                        }
                    }
                    MarkAsCrawled(url);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception:  '{ex.Message}'");
            }

        }

        // this method cralws out put HTML of https://www.zillow.com/browse/homes/wa/king-county/newest-homes/
        private const string XPATH_UL_BODYLINKS_LI_ANCHOR_LINKS = "//ul[@class='bh-body-links']/li/a";
        private void ExtractAndQueueCountyScopedSaleUrls(string url)
        {
            try
            {
                HtmlDocument htmlDoc = CrawlUtils.getHtmlDocFromUrl(url);

                // Let us test for  US states only for now.. 
                var propertyNodes = htmlDoc.DocumentNode.SelectNodes(XPATH_UL_BODYLINKS_LI_ANCHOR_LINKS);

                if (propertyNodes == null)
                {
                    Console.WriteLine($" Unable to get Property URLs from this URL: {url}");
                    var node = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
                    Console.WriteLine("Node Name: " + node.Name + "\n" + node.OuterHtml);
                }
                else
                {

                    foreach (var node in propertyNodes)
                    {
                        //Console.WriteLine(" SubNode Name: " + node.Name + "\n" + node.InnerHtml + " Outter"+ node.OuterHtml);
                        var propertyUrl = UrlUtils.GetAbsoluteUrlString(url, node.Attributes["href"].Value);

                        switch (getTypeOfURL(propertyUrl))
                        {
                            case ContentHoldingByURI.PROPERTY:
                                AddUrlToTheQueue(propertyUrl);
                                break;
                            case ContentHoldingByURI.MISC:
                                break;
                            default:
                                Console.WriteLine($"This URL should not be in scope of {ContentHoldingByURI.COUNTYSALEHOMES}, url:{propertyUrl}");
                                break;
                        }
                    }
                    MarkAsCrawled(url);
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception:  '{ex.Message}'");
            }
        }

        // this method cralws out put HTML of https://www.zillow.com/browse/homes/wa/king-county
        private void ExtractAndQueueCountyUrls(Uri uri)
        {

        }

        //https://www.zillow.com/browse/homes/wa/king-county/98052
        private void ExtractAndQueueZipCodeUrls(Uri uri)
        {

        }

        //https://www.zillow.com/browse/homes/wa/king-county/98052/12/
        private void ExtractAndQueueZipCodeExtUrls(Uri uri)
        {

        }

        // Extract Property details.. https://www.zillow.com/homedetails/0-11875-175th-Pl-NE-Redmond-WA-98052/2141039322_zpid/
        private void ExtractPropertyInfoAndQueuethemForAnalysis( string url)
        {
            try
            {
                HtmlDocument htmlDoc = CrawlUtils.getHtmlDocFromUrl(url);
                Console.WriteLine($"Crawled proprty URL: {url}");
                
                MarkAsCrawled(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception:  '{ex.Message}'");
            }

        }
        #endregion

        public void CrawlProperties(Uri inputUri)
        {
            // determine URL scope .. 
            // https://www.zillow.com/browse/homes:  Each State date
            // https://www.zillow.com/browse/homes/wa/  : State specific .. list contain County properties and Sale Properties.
            // https://www.zillow.com/browse/homes/wa/king-county/newest-homes/ : List of Sale homes
            // https://www.zillow.com/homedetails/0-11875-175th-Pl-NE-Redmond-WA-98052/2141039322_zpid/ : Property

            urlQueue.Enqueue(inputUri);

            while (urlQueue.Count > 0)
            {
                // Basic Checks                
                Uri zillowUri;
                if (!urlQueue.TryDequeue(out zillowUri))
                {
                    Console.WriteLine(" Null Item from Queue.. Really Strange");
                    continue;
                }

                switch (getTypeOfURL(zillowUri))
                {
                    case ContentHoldingByURI.PROPERTY:
                        ExtractPropertyInfoAndQueuethemForAnalysis(zillowUri.AbsoluteUri);
                        break;
                    case ContentHoldingByURI.STATES:
                        ExtractAndQueueStateUrls(zillowUri.AbsoluteUri);
                        break;
                    case ContentHoldingByURI.COUNTY:
                        ExtractAndQueueCountyUrls(zillowUri.AbsoluteUri);
                        break;
                    case ContentHoldingByURI.COUNTYSALEHOMES:
                        ExtractAndQueueCountyScopedSaleUrls(zillowUri.AbsoluteUri);
                        break;
                    case ContentHoldingByURI.COUNTYZIPCODE:
                        break;
                    case ContentHoldingByURI.ZIPCODEEXT:
                        break;
                    case ContentHoldingByURI.ZIPCODEEXTPROPS:
                        break;
                    case ContentHoldingByURI.MISC:
                        break;
                }

            }
        }
    }
}
