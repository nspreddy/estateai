using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModels;
using CommonAndUtils;
using RedfinUtils;

namespace CrawlProperties
{
    internal class PrepareListAndCrawl
    {
        private CrawlerInputConfig crawlerConfig { get; set; }
        private string XmlInDir { get; set; }
        private string XmlOutDir { get; set; }

        private HashSet<string> Cities2Crawl { get; set; }

        private HashSet<string> zipCodes2Crawl { get; set; }

        private HashSet<string> PropertyUrls2Crawl { get; set; }

        public PrepareListAndCrawl(CrawlerInputConfig crawlConfig, string inDir, string outDir)
        {
            crawlerConfig= crawlConfig;
            XmlInDir           = inDir;
            XmlOutDir          = outDir;
            Cities2Crawl       = new HashSet<string>();
            zipCodes2Crawl     = new HashSet<string>();
            PropertyUrls2Crawl = new HashSet<string>();
        }

        public bool PrepareList2Crawl()
        {
            bool returnValue = false;

            var stateObject = GeoData.DefaultNation.GetState(crawlerConfig.State);
            if (stateObject != null)
            {
                // get cities and zipcodes in Hash set ( look up is faster.. O(1) incase of unique hash keys
                UpdateHashSetWithCityList(crawlerConfig.CityList);
                UpdateHashSetWithZipCodeList(crawlerConfig.ZipCodeList);

                // convert counties to cities
                foreach ( var county in crawlerConfig.CountyList)
                {
                    var listofCities = stateObject.GetListofCitiesInaCounty(county);
                    UpdateHashSetWithCityList(listofCities);
                }
                Console.WriteLine($"Cities total: {Cities2Crawl.Count()} read to Crawl");
                // Let us get XML file path with URLs of properties to CRAWL.
                var xmlPropUrlFile = stateObject.GetFilePathWithRelativeDirPath(XmlInDir, State.SALE_LISTINGS_PREFIX);
                if (xmlPropUrlFile != null)
                {
                    returnValue = ComputeShortListofUrls(xmlPropUrlFile);
                }
                else
                {
                    Console.WriteLine($"Failed to get XMlFile with prop URLs");
                }

            }
            else
            {
                Console.WriteLine($"State:{crawlerConfig.State} not found in the given GeoData");
            }
            return returnValue;
        }

        public bool StartCrawling()
        {
            bool returnValue = false;
            foreach( var url2Crawl in PropertyUrls2Crawl)
            {
                Console.WriteLine($" Kicking off to Crawl {url2Crawl}");
            }

            return returnValue;
        }

        #region PRIVATE
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cities"></param>
        private void  UpdateHashSetWithCityList( List<string> cities)
        {
            foreach( var city in cities)
            {
                var cityTrimmed = city.ToLower().Trim();
                if ( !Cities2Crawl.Contains(cityTrimmed)){
                    Cities2Crawl.Add(cityTrimmed);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipCodeList"></param>
        private void UpdateHashSetWithZipCodeList(List<string> zipCodeList)
        {
            foreach (var zipCode in zipCodeList)
            {
                var zipCodeTrimmed = zipCode.ToLower().Trim();
                if (!zipCodes2Crawl.Contains(zipCodeTrimmed))
                {
                    zipCodes2Crawl.Add(zipCodeTrimmed);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        private void AddPropUrl( string url)
        {
            if(!PropertyUrls2Crawl.Contains(url))
            {
                PropertyUrls2Crawl.Add(url);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private const string XPATH_PROP_LINKS = "//loc";
        private bool ComputeShortListofUrls(string xmlFilenameWithURLs)
        {
            bool returnValue = false;

            try
            {
                var xmlDoc = CrawlUtils.GethtmlDocFromXmlFile(xmlFilenameWithURLs);
                if(xmlDoc != null)
                {
                    // Let us get list of URLs
                    var propertyUrlNodes = xmlDoc.DocumentNode.SelectNodes(XPATH_PROP_LINKS);

                    if (propertyUrlNodes != null)
                    {

                        foreach (var propNode in propertyUrlNodes)
                        {
                            var propLinkUrl = propNode.InnerText;
                            //Console.WriteLine($"Prop-Link: {propLinkUrl}");
                            var urlKeyElements = RedfinPropURLUtils.ParseRedfinPropURL(propLinkUrl);
                            if(urlKeyElements != null && urlKeyElements.Count > 0)
                            {
                                var City    = urlKeyElements[RedfinPropURLUtils.CITY];
                                var ZipCode = urlKeyElements[RedfinPropURLUtils.ZIPCODE];
                                bool isPropCrawlable = false;

                                if (!string.IsNullOrEmpty(City) && Cities2Crawl.Contains(City))
                                {
                                    isPropCrawlable = true;
                                }else if (!string.IsNullOrEmpty(ZipCode) && zipCodes2Crawl.Contains(ZipCode))
                                {
                                    isPropCrawlable = true;
                                }

                                if (isPropCrawlable)
                                {
                                    AddPropUrl(propLinkUrl);
                                }
                                else
                                {
                                    //Console.WriteLine($"Not adding the URL {propLinkUrl}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Not adding the URL {propLinkUrl} due to parsing Issues");
                            }
                            
                        }
                        returnValue = true;

                    }
                    else
                    {
                        Console.WriteLine($"Unabel to get property URls from XML Content");
                    }
                }
                else
                {
                    Console.WriteLine($"Unbale to read XML file {xmlFilenameWithURLs}");
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception while reading/understanding  {xmlFilenameWithURLs}, exception: {ex.Message}");
            }

            return returnValue;
        }
        #endregion
    }
}
