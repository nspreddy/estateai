using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonAndUtils;
using Newtonsoft.Json;
using System.IO;

namespace DataModels
{
    public class State
    {
        public const string SALE_LISTINGS_PREFIX = "SaleListings";
        public const string COUNTY_STATS_PREFIX = "CountyStats";
        public const string CITY_STATS_PREFIX = "CityStats";
        public const string ZIPCODE_STATS_PREFIX = "ZipCodeStats";

        public const string META_DATA  = "MetaData";
        public const string PROP_DATA  = "PropData";
        public const string STATS_DATA = "StatsData";

        [JsonProperty(PropertyName = "County-Data")]
        public Dictionary<string,County> CountyList = new Dictionary<string,County>();
        [JsonProperty(PropertyName = "StateCode")]
        public string Name { get; set; }

        [JsonIgnore]
        private Nation Parent { get; set; }

        public State() { }
        public State( string name, Nation nation)
        {
            Name   = name;
            Parent = nation;
        }

        public void ValidateParent(Nation nation)
        {
            if( Parent == null)
            {
                Parent = nation;
            }
            foreach( var kv in CountyList)
            {
                kv.Value?.ValidateParent(this);                
            }
        }

        public bool InsertGeoRecord( string county, string city, string zipcode)
        {
            bool returnValue = false;

            try
            {
                County countyObject;

                if (!CountyList.TryGetValue(county, out countyObject))
                {
                    countyObject = new County(county,this);
                    CountyList[county] = countyObject;
                }
                returnValue = countyObject.InsertGeoRecord(city,zipcode);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to insert geoRecord {county}, {city},{zipcode} exception :{e.Message}");
            }

            return returnValue;
        }

        public bool GenerateCriteriaConfigurationTemplates(string fileprefix,string dir)
        {
            bool returnValue = false;

            try
            {
                // Let us create a direcrtory strcture .. <dir>/<county>/<state>/<county>/<city>/<zipcode>               
                var computedDir = Path.Combine(dir, Name);

                if (CountyList.Count > 0)
                {
                    // Let us generate CrawlConfiguraions First
                    var filename = $"CrawlConfig_{fileprefix}.json";
                    GenerateCrawlConfigTempl(computedDir, filename);
                    foreach (var kv in CountyList)
                    {
                        var county = kv.Key;
                        var countyObject = kv.Value;
                        Console.WriteLine($"Calling County  {county} to create configuration");
                        var prefix = $"ProcessorConfig_{fileprefix}_{county}";
                        countyObject.GenerateCriteriaConfigurationTemplates(Name,prefix, computedDir);
                    }
                    returnValue = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception  While genrating config templates(Nation Object) {ex.Message}");
            }

            return returnValue;
        }

        public List<string> GetListofCitiesInaCounty(string county)
        {
            List<string> result = null;
            try
            {
                County countyObject;

                if (CountyList.TryGetValue(county, out countyObject))
                {
                  result = countyObject.CityList.Keys.ToList();
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to insert geoRecord {county}, Exception :{e.Message}");
            }
            return result;
        }

        public string GetFilePathForMetaData(string headDir,string filenameSuffix)
        {
            return GetFilePathWithHeadDir(headDir, META_DATA, filenameSuffix);
        }

        public string GetFilePathForPropertyData(string headDir, string filenameSuffix)
        {
            return GetFilePathWithHeadDir(headDir, PROP_DATA, filenameSuffix);
        }

        public string GetFilePathForStatsData(string headDir, string filenameSuffix)
        {
            return GetFilePathWithHeadDir(headDir, STATS_DATA, filenameSuffix);
        }

        public string GetFilePathWithRelativeDirPath(string dirPath, string fileSuffix)
        {
            var filename = $"{Name}_{fileSuffix}";            
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            return Path.Combine(dirPath, filename);
        }

        #region PRIVATE
        private const int MAX_RECORDS = 3; 
        // generate crawl confir (property or Stats.. )
        private void GenerateCrawlConfigTempl(string dir, string filename)
        {
            try
            {
                var crawlCfg = new CrawlerInputConfig();
                crawlCfg.JobName = $"CrawlJob_{Name}";
                crawlCfg.State = Name;
                // add some sample counties( max as 3)
                if (CountyList.Count > 0)
                {
                    int i = 0;
                    foreach (var countyKV in CountyList)
                    {
                        crawlCfg.CountyList.Add(countyKV.Key);
                        var CountyObject = countyKV.Value;
                        if (CountyObject != null)
                        {
                            // Let us fill few cities. 
                            int j = 0;
                            foreach (var cityKV in CountyObject.CityList)
                            {
                                crawlCfg.CityList.Add(cityKV.Key);
                                var cityObject = cityKV.Value;
                                int k = 0;
                                foreach (var zipKV in cityObject.ZipCodeList)
                                {
                                    crawlCfg.ZipCodeList.Add(zipKV.Key);
                                    ++k;
                                    if (k >= MAX_RECORDS) break;
                                }
                                ++j;
                                if (j >= MAX_RECORDS) break;
                            }
                        }
                        ++i;
                        if (i >= MAX_RECORDS) break;
                    }
                    // Write to JSON File. 
                    string jsonPayload = JsonConvert.SerializeObject(crawlCfg, Formatting.Indented);
                    FileReadWriteUtil.WriteToFile(dir, filename, jsonPayload);
                }                
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception  While Writing County Configuration to JSON file {ex.Message}");
            }              
        }

        /// <summary>
        /// Get Path of the file name based on the scope/file prefix.
        /// </summary>
        /// <param name="outDir"></param>
        /// <param name="filePrefix"></param>
        /// <returns></returns>
        private string GetFilePathWithHeadDir(string headDir, string subdir, string filenameSuffix)
        {
            var filename = $"{Name}_{filenameSuffix}";
            var dateFolderName = DateTime.Now.ToString("yyyy_MM_dd");
            var dir = Path.Combine(headDir, this.Parent.Name, Name, dateFolderName, subdir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return Path.Combine(dir, filename);
        }

        #endregion

        #region URL_BASED_APIS
        // Crawl URLS
        // City Based Stats:  URL pattern by State:
        //   WA:  https://www.redfin.com/sitemap-xml/city_housing_market_WA_sitemap.xml.gz
        //   Parent URL has all of these State URLs is https://www.redfin.com/city_housing_market_sitemap.xml


        // County Based Stats
        // Parent URL holding all States: https://www.redfin.com/county_housing_market_sitemap.xml
        // ex WA:  https://www.redfin.com/sitemap-xml/county_housing_market_WA_sitemap.xml.gz

        // zipcode based
        // Parent URL for all States : https://www.redfin.com/zipcode_housing_market_sitemap.xml
        // ex: WA https://www.redfin.com/sitemap-xml/zipcode_housing_market_WA_sitemap.xml.gz

        // Neighbourhoodl based 
        // Parent URL for all States :https://www.redfin.com/neighborhoods_housing_market_sitemap.xml
        // ex: WA https://www.redfin.com/sitemap-xml/neighborhoods_housing_market_WA_sitemap.xml.gz

        private const string CITY_SCOPED_STATS = "https://www.redfin.com/sitemap-xml/city_housing_market_{0}_sitemap.xml.gz";
        private const string COUNTY_SCOPED_STATS = "https://www.redfin.com/sitemap-xml/county_housing_market_{0}_sitemap.xml.gz";
        private const string ZIPCODE_SCOPED_STATS = "https://www.redfin.com/sitemap-xml/zipcode_housing_market_{0}_sitemap.xml.gz";
        private const string NBHOOD_SCOPED_STATS = "https://www.redfin.com/sitemap-xml/neighborhoods_housing_market_{0}_sitemap.xml.gz";

        public string CityStatsUrl
        {
            get
            {
                return String.Format(CITY_SCOPED_STATS, Name);
            }
        }

        public string CountyStatsUrl
        {
            get
            {
                return String.Format(COUNTY_SCOPED_STATS, Name);
            }
        }

        public string ZipCodeStatsUrl
        {
            get
            {
                return String.Format(ZIPCODE_SCOPED_STATS, Name);
            }
        }
        // Listings: 
        // Parent: https://www.redfin.com/listings_sitemap.xml
        // ex: MA https://www.redfin.com/listings_MA_sitemap.xml
        // ex: https://www.redfin.com/sitemap-xml/listings_MA_sitemap.xml.gz
        private const string SALELIST_STATE_SCOPED = "https://www.redfin.com/sitemap-xml/listings_{0}_sitemap.xml.gz";

        public string SalesListUrl
        {
            get
            {
                return String.Format(SALELIST_STATE_SCOPED, Name);
            }
        }

        // Property (all homes): 
        // Parent: https://www.redfin.com/properties_sitemap.xml
        // ex: MA  https://www.redfin.com/sitemap-xml/properties_MA_sitemap5.xml.gz
        private const string PROPLIST_STATE_SCOPED = "https://www.redfin.com/sitemap-xml/properties_{0}_sitemap5.xml.gz";

        public string PropsListUrl
        {
            get
            {
                return String.Format(PROPLIST_STATE_SCOPED, Name);
            }
        }

        // TBD - Future
        // Updates(Delta): 
        // Parent: https://www.redfin.com/latest_updates.xml
        // ex: nothing state specific .. https://www.redfin.com/sitemap-xml/latest_updates_2.xml.gz

        // Property (Newest Since last call): 
        // https://www.redfin.com/newest_listings.xml
        // Gives properties across US.. (similar output as https://www.redfin.com/listings_MA_sitemap.xml)

        #endregion
    }
}
