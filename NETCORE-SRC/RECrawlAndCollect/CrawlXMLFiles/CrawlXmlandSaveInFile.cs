using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModels;
using CrawlerLib;
using System.IO;

namespace CrawlXMLFiles
{
    public class CrawlXmlandSaveInFile
    {
        public string OutputDir { get; private set; }
        public CrawlXmlandSaveInFile(string ourDir)
        {
            OutputDir = ourDir;
        }

        public bool CrawlAllStates(string crawlType)
        {
            bool returnValue = false;

            try
            {
                var statesList = GeoData.DefaultNation.GetListOfStates();
                if( statesList != null)
                {
                    foreach( var stateCode in statesList)
                    {
                        CrawlSpecificState(stateCode, crawlType);
                    }
                }
                else
                {
                    Console.WriteLine($"Empty State List");
                }
            }
            catch(Exception ex)
            {               
                Console.WriteLine($"Exception while Crawling and saving XML files for all states, exception: {ex.Message}");
            }

            return returnValue;

        }

        public bool CrawlSpecificState(string stateCode, string crawlType)
        {
            bool returnValue = false;

            try
            {
                var stateObject = GeoData.DefaultNation.GetState(stateCode);
                if( stateObject != null)
                {
                    bool crawlListings = false;
                    bool crawlStats = false;

                    switch (crawlType)
                    {
                        case CrawlXmlsFilesMain.CRAWL_ALL:
                            crawlListings = true;
                            crawlStats = true;
                            break;
                        case CrawlXmlsFilesMain.CRAWL_STATS:
                            crawlStats = true;
                            break;
                        case CrawlXmlsFilesMain.CRAWL_LIST:
                            crawlListings = true;
                            break;

                    }
                    var nation = GeoData.DefaultNation.Name;
                    if (crawlListings)
                    {
                        CrawlAndSave(nation, stateObject, SALE_LISTINGS_PREFIX);
                    }

                    if (crawlStats)
                    {
                        CrawlAndSave(nation, stateObject, COUNTY_STATS_PREFIX);
                        CrawlAndSave(nation, stateObject, CITY_STATS_PREFIX);
                        CrawlAndSave(nation, stateObject, ZIPCODE_STATS_PREFIX);
                    }
                    returnValue = true;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while Crawling and saving XML files for  State { stateCode}, exception:  {ex.Message}");
            }

            return returnValue;
        }

        #region PRIVATE_HELPERS       

        private const string SALE_LISTINGS_PREFIX = "SaleListings";
        private const string COUNTY_STATS_PREFIX = "CountyStats";
        private const string CITY_STATS_PREFIX = "CityStats";
        private const string ZIPCODE_STATS_PREFIX = "ZipCodeStats";


        private bool CrawlAndSave(string nation,State stateObject,string prefix )
        {
            bool returnValue = false;
            string url2Crawl = "";
            if (stateObject != null)
            {
                switch (prefix)
                {
                    case SALE_LISTINGS_PREFIX:
                        url2Crawl = stateObject.SalesListUrl;
                        break;
                    case COUNTY_STATS_PREFIX:
                        url2Crawl = stateObject.CountyStatsUrl;
                        break;
                    case CITY_STATS_PREFIX:
                        url2Crawl = stateObject.CityStatsUrl;
                        break;
                    case ZIPCODE_STATS_PREFIX:
                        url2Crawl = stateObject.ZipCodeStatsUrl;
                        break;
                }

                if (!string.IsNullOrEmpty(url2Crawl))
                {
                    var filePath = GetFullFilePath(nation, stateObject.Name, prefix);

                    returnValue = CrawlerFramework.QueueCrawlRedfinXMLContentJob(url2Crawl, filePath);
                    /*
                    var crawler = new Crawler();
                    returnValue = crawler.CrawlRedfinXMLContent(url2Crawl, filePath);*/
                    if(!returnValue)
                    {
                        Console.WriteLine($"Unable to Crawl {url2Crawl} for {prefix}");
                    }
                }
            }
            return returnValue;
        }
        private string GetFullFilePath(string nation, string state, string filePrefix)
        {
            var filename = $"{filePrefix}_{state}.xml";
            var dateFolderName = DateTime.Now.ToString("yyyy_MM_dd");
            var dir = Path.Combine(OutputDir, nation, state, dateFolderName);
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return Path.Combine(dir, filename);
        }
        #endregion
    }
}
