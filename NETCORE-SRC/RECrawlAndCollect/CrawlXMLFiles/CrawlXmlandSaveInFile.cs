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
                    
                    if (crawlListings)
                    {
                        CrawlAndSave(stateObject, State.SALE_LISTINGS_PREFIX);
                    }

                    if (crawlStats)
                    {
                        CrawlAndSave(stateObject, State.COUNTY_STATS_PREFIX);
                        CrawlAndSave(stateObject, State.CITY_STATS_PREFIX);
                        CrawlAndSave(stateObject, State.ZIPCODE_STATS_PREFIX);
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

        private bool CrawlAndSave(State stateObject,string prefix )
        {
            bool returnValue = false;
            string url2Crawl = "";
            if (stateObject != null)
            {
                switch (prefix)
                {
                    case State.SALE_LISTINGS_PREFIX:
                        url2Crawl = stateObject.SalesListUrl;
                        break;
                    case State.COUNTY_STATS_PREFIX:
                        url2Crawl = stateObject.CountyStatsUrl;
                        break;
                    case State.CITY_STATS_PREFIX:
                        url2Crawl = stateObject.CityStatsUrl;
                        break;
                    case State.ZIPCODE_STATS_PREFIX:
                        url2Crawl = stateObject.ZipCodeStatsUrl;
                        break;
                }

                if (!string.IsNullOrEmpty(url2Crawl))
                {
                    //var filePath = GetFullFilePath(nation, stateObject.Name, prefix);
                    var fileSuffix = $"{prefix}.xml";
                    var filePath = stateObject.GetFilePathForMetaData(OutputDir,"", fileSuffix);

                    returnValue = CrawlerFramework.QueueCrawlUrAndSave2FileJob(url2Crawl, filePath);

                    if(!returnValue)
                    {
                        Console.WriteLine($"Unable to Crawl {url2Crawl} for {prefix}");
                    }
                }
            }
            return returnValue;
        }
                
        #endregion
    }
}
