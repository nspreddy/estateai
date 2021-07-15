using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModels;
using CommonAndUtils;
using RedfinUtils;
using CrawlerLib;

namespace CrawlProperties
{
    internal class CrawlHelper
    {
        #region PUBLIC_MEMEBERS
        public const string CRAWL_PROPS_CMD = "props";
        public const string CRAWL_STATS_CMD = "stats";
        #endregion

        #region PRIVATE_MEMBERS
        private string Command { get; set; }
        private CrawlerInputConfig crawlerConfig { get; set; }
        private string InDir { get; set; }
        private string OutDir { get; set; }

        private HashSet<string> Counties2Crawl { get; set; }
        private HashSet<string> Cities2Crawl { get; set; }

        private HashSet<string> ZipCodes2Crawl { get; set; }

        private Dictionary<string,string> PropertyUrls2Crawl { get; set; }
        #endregion

        #region PUBLIC_METHODS
        public CrawlHelper(string command, CrawlerInputConfig crawlConfig, string inDir, string outDir)
        {
            Command = command;
            crawlerConfig= crawlConfig;
            InDir           = inDir;
            OutDir          = outDir;
            Cities2Crawl       = new HashSet<string>();
            ZipCodes2Crawl     = new HashSet<string>();
            Counties2Crawl     = new HashSet<string>();
            PropertyUrls2Crawl = new Dictionary<string, string>();
        }

        public bool PrepareList2Crawl()
        {
            bool returnValue = false;
            switch (Command)
            {
                case CRAWL_PROPS_CMD:
                    returnValue=  PreparePropertyList2Crawl();
                    break;
                case CRAWL_STATS_CMD:
                    returnValue = PrepareStatsList2Crawl();
                    break;
                default:
                    Console.WriteLine($"Wrong Command: {Command}");
                    break;
            }
            return returnValue;
        }
        
        public void QueueCrawlJobs()
        {
            Console.WriteLine(" Queuing Crawl  and Save Jobs");
            foreach( var url2CrawlKvPair in PropertyUrls2Crawl)
            {
                Console.WriteLine($" Kicking off to Crawl {url2CrawlKvPair.Value}");
                var fileSuffix = $"{url2CrawlKvPair.Key}.html";
                var stateObject = GeoData.DefaultNation.GetState(crawlerConfig.State);

                if(stateObject != null)
                {
                    string filePath = "";
                    switch (Command)
                    {
                        case CRAWL_PROPS_CMD:
                            filePath = stateObject.GetFilePathForPropertyData(OutDir, fileSuffix);
                            break;
                        case CRAWL_STATS_CMD:
                            filePath = stateObject.GetFilePathForStatsData(OutDir, fileSuffix);
                            break;
                        default:
                            Console.WriteLine($" Unknown Cmd ({Command}) area to process");
                            break;
                    }
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        bool returnValue = CrawlerFramework.QueueCrawlUrAndSave2FileJob(url2CrawlKvPair.Value, filePath);
                        if (!returnValue)
                        {
                            Console.WriteLine($"Unable to Crawl {url2CrawlKvPair.Value} sane Save to  {filePath}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Unable to Get State Object for State:{crawlerConfig.State}");
                }                
            } 
        }
        #endregion

        #region PRIVATE

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool PrepareStatsList2Crawl()
        {
            bool returnValue = false;

            var stateObject = GeoData.DefaultNation.GetState(crawlerConfig.State);
            if (stateObject != null)
            {
                UpdateHashSetWithCountyList(crawlerConfig.CountyList);
                UpdateHashSetWithCityList(crawlerConfig.CityList);
                UpdateHashSetWithZipCodeList(crawlerConfig.ZipCodeList);
                ComputeShortlistOfCountyStatsURLs();
                ComputeShortlistOfCityStatsURLs();
                ComputeShortlistOfZipCodeStatsURLs();
                returnValue = true;
            }
            else
            {
                Console.WriteLine($"State:{crawlerConfig.State} not found in the given GeoData");
            }
            return returnValue;
        }
        /// <summary>
        /// Compute, Filter and add County Stats URLs to crawl
        /// </summary>
        private void ComputeShortlistOfCountyStatsURLs()
        {
            try
            {
                var stateObject = GeoData.DefaultNation.GetState(crawlerConfig.State);
                var countyStatsXmlFilePrefix = $"{State.COUNTY_STATS_PREFIX}.xml";
                var countyXmlUrlFile = stateObject.GetFilePathWithRelativeDirPath(InDir, countyStatsXmlFilePrefix);

                var listOfUrls2Filter = GetUrls2CrawlFromXmlFile(countyXmlUrlFile);

                if (listOfUrls2Filter != null && listOfUrls2Filter.Count > 0)
                {
                    foreach (var url in listOfUrls2Filter)
                    {
                        // sample county Stats URL: https://www.redfin.com/county/2/WA/Snohomish-County/housing-market
                        var urlKeyElements = RedfinPropURLUtils.ParseRedfinCountyStatURL(url);
                        if (urlKeyElements != null && urlKeyElements.Count > 0)
                        {
                            var state = urlKeyElements[RedfinPropURLUtils.STATE];
                            var county = urlKeyElements[RedfinPropURLUtils.COUNTY];

                            bool isPropCrawlable = false;

                            if (!string.IsNullOrEmpty(county) && Counties2Crawl.Contains(county))
                            {
                                isPropCrawlable = true;
                            }

                            if (isPropCrawlable)
                            {
                                // Let us create a Key                          
                                var propKey = $"{state}_countystats_{county}";
                                AddPropUrl(propKey, url);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Not adding the URL {url} due to parsing Issues");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"No list to Crawl for Counties");
                }
            }catch (Exception ex)
            {
                Console.WriteLine($"Exception while processing County Stats URLs, exception:{ex.Message}");
            }

        }

        private void ComputeShortlistOfCityStatsURLs()
        {
            try
            {
                var stateObject = GeoData.DefaultNation.GetState(crawlerConfig.State);
                var cityStatsXmlFilePrefix = $"{State.CITY_STATS_PREFIX}.xml";
                var cityXmlUrlFile = stateObject.GetFilePathWithRelativeDirPath(InDir, cityStatsXmlFilePrefix);

                var listOfUrls2Filter = GetUrls2CrawlFromXmlFile(cityXmlUrlFile);
                if (listOfUrls2Filter != null && listOfUrls2Filter.Count > 0)
                {
                    foreach (var url in listOfUrls2Filter)
                    {
                        // sample City  Stats URL: https://www.redfin.com/city/13/WA/Aberdeen/housing-market 
                        var urlKeyElements = RedfinPropURLUtils.ParseRedfinCityStatURL(url);
                        if (urlKeyElements != null && urlKeyElements.Count > 0)
                        {
                            var state = urlKeyElements[RedfinPropURLUtils.STATE];
                            var city = urlKeyElements[RedfinPropURLUtils.CITY];

                            bool isPropCrawlable = false;
                            if (!string.IsNullOrEmpty(city) && Cities2Crawl.Contains(city))
                            {
                                isPropCrawlable = true;
                            }

                            if (isPropCrawlable)
                            {
                                // Let us create a Key                          
                                var propKey = $"{state}_citystats_{city}";
                                AddPropUrl(propKey, url);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Not adding the URL {url} due to parsing Issues");
                        }
                    }

                }
                else
                {
                    Console.WriteLine($"No list to Crawl for Counties");
                }
            }catch (Exception ex)
            {
                Console.WriteLine($"Exception while processing CityStats URLs, exception:{ex.Message}");
            }

        }

        private void ComputeShortlistOfZipCodeStatsURLs()
        {
            try
            {
                var stateObject = GeoData.DefaultNation.GetState(crawlerConfig.State);
                var zipCodeStatsXmlFilePrefix = $"{State.ZIPCODE_STATS_PREFIX}.xml";
                var zipcodeXmlUrlFile = stateObject.GetFilePathWithRelativeDirPath(InDir, zipCodeStatsXmlFilePrefix);

                var listOfUrls2Filter = GetUrls2CrawlFromXmlFile(zipcodeXmlUrlFile);
                if (listOfUrls2Filter != null && listOfUrls2Filter.Count > 0)
                {
                    foreach (var url in listOfUrls2Filter)
                    {
                        // sample ZipCode  Stats URL: https://www.redfin.com/zipcode/98001/housing-market 
                        var urlKeyElements = RedfinPropURLUtils.ParseRedfinZipcodeStatURL(url);
                        if (urlKeyElements != null && urlKeyElements.Count > 0)
                        {
                            var zipcode = urlKeyElements[RedfinPropURLUtils.ZIPCODE];

                            bool isPropCrawlable = false;
                            if (!string.IsNullOrEmpty(zipcode) && ZipCodes2Crawl.Contains(zipcode))
                            {
                                isPropCrawlable = true;
                            }

                            if (isPropCrawlable)
                            {
                                // Let us create a Key                          
                                var propKey = $"{stateObject.Name}_zipcodestats_{zipcode}";
                                AddPropUrl(propKey, url);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Not adding the URL {url} due to parsing Issues");
                        }

                    }
                }
                else
                {
                    Console.WriteLine($"No list to Crawl for Counties");
                }
            }catch (Exception ex)
            {
                Console.WriteLine($"Exception while processing ZipCodeStats URLs, exception:{ex.Message}");
            }

        }
        /// <summary>
        /// Property List prep code
        /// </summary>
        /// <returns></returns>
        private bool PreparePropertyList2Crawl()
        {
            bool returnValue = false;

            var stateObject = GeoData.DefaultNation.GetState(crawlerConfig.State);
            if (stateObject != null)
            {
                // get cities and zipcodes in Hash set ( look up is faster.. O(1) incase of unique hash keys
                UpdateHashSetWithCityList(crawlerConfig.CityList);
                UpdateHashSetWithZipCodeList(crawlerConfig.ZipCodeList);

                // convert counties to cities
                foreach (var county in crawlerConfig.CountyList)
                {
                    var listofCities = stateObject.GetListofCitiesInaCounty(county);
                    UpdateHashSetWithCityList(listofCities);
                }
                Console.WriteLine($"Cities total: {Cities2Crawl.Count()} read to Crawl");
                // Let us get XML file path with URLs of properties to CRAWL.
                var fileSuffix = $"{State.SALE_LISTINGS_PREFIX}.xml";
                var xmlPropUrlFile = stateObject.GetFilePathWithRelativeDirPath(InDir, fileSuffix);
                if (xmlPropUrlFile != null)
                {
                    returnValue = ComputeShortListofPropertyUrls(xmlPropUrlFile);
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

        /// <summary>
        /// 
        /// </summary>
        
        private bool ComputeShortListofPropertyUrls(string xmlFilenameWithURLs)
        {
            bool returnValue = false;

            try
            {
                var listOfUrls2Filter = GetUrls2CrawlFromXmlFile(xmlFilenameWithURLs);
                if (listOfUrls2Filter != null && listOfUrls2Filter.Count > 0)
                {
                    foreach (var propLinkUrl in listOfUrls2Filter)
                    {
                        // sample Property URL : https://www.redfin.com/WA/Moses-Lake/7935-Dune-Lake-Rd-SE-98837/home/16825225
                        var urlKeyElements = RedfinPropURLUtils.ParseRedfinPropURL(propLinkUrl);
                        if (urlKeyElements != null && urlKeyElements.Count > 0)
                        {
                            var City = urlKeyElements[RedfinPropURLUtils.CITY];
                            var ZipCode = urlKeyElements[RedfinPropURLUtils.ZIPCODE];

                            bool isPropCrawlable = false;

                            if (!string.IsNullOrEmpty(City) && Cities2Crawl.Contains(City))
                            {
                                isPropCrawlable = true;
                            }
                            else if (!string.IsNullOrEmpty(ZipCode) && ZipCodes2Crawl.Contains(ZipCode))
                            {
                                isPropCrawlable = true;
                            }

                            if (isPropCrawlable)
                            {
                                // Let us create a Key                                   
                                var state = urlKeyElements[RedfinPropURLUtils.STATE];
                                var address = urlKeyElements[RedfinPropURLUtils.ADDRESS];
                                var Id = urlKeyElements[RedfinPropURLUtils.PROPID];

                                var propKey = $"{state}_{City}_{Id}_{address}";
                                AddPropUrl(propKey, propLinkUrl);
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

                }
                else
                {
                    Console.WriteLine($"No list to Crawl for Properties");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception while reading/understanding  {xmlFilenameWithURLs}, exception: {ex.Message}");
            }          
            
            return returnValue;
        }

        private const string XPATH_PROP_LINKS = "//loc";
        private List<string> GetUrls2CrawlFromXmlFile(string xmlFilenameWithURLs)
        {
            List<string> returnValue = new List<string>();

            try
            {
                var xmlDoc = CrawlUtils.GethtmlDocFromXmlFile(xmlFilenameWithURLs);
                if (xmlDoc != null)
                {
                    // Let us get list of URLs
                    var propertyUrlNodes = xmlDoc.DocumentNode.SelectNodes(XPATH_PROP_LINKS);
                    foreach (var propNode in propertyUrlNodes)
                    {
                        var propLinkUrl = propNode.InnerText;
                        if(!string.IsNullOrEmpty(propLinkUrl))
                        {
                            returnValue.Add(propLinkUrl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception While reading the file: {xmlFilenameWithURLs}, Exception: {ex.Message}");
            }
            return returnValue;
        }

        private void UpdateHashSetWithCountyList(List<string> counties)
        {
            foreach (var county in counties)
            {
                var countyTrimmed = county.ToLower().Trim();
                if (!Counties2Crawl.Contains(countyTrimmed))
                {
                    Counties2Crawl.Add(countyTrimmed);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cities"></param>
        private void UpdateHashSetWithCityList(List<string> cities)
        {
            foreach (var city in cities)
            {
                var cityTrimmed = city.ToLower().Trim();
                if (!Cities2Crawl.Contains(cityTrimmed))
                {
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
                if (!ZipCodes2Crawl.Contains(zipCodeTrimmed))
                {
                    ZipCodes2Crawl.Add(zipCodeTrimmed);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        private void AddPropUrl(string key, string url)
        {
            if (!PropertyUrls2Crawl.ContainsKey(key))
            {
                PropertyUrls2Crawl.Add(key, url);
            }
        }

        #endregion
    }
}
