using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModels;
using CommonAndUtils;
using RedfinUtils;
using CrawlerLib;
using System.IO;

namespace PropAnalyzer
{
    internal class AnalysisHelper
    {

        #region PRIVATE_MEMBERS
        private string Command { get; set; }
        private CrawlerInputConfig ProcessorCfg { get; set; }
        private string InDir { get; set; }
        private string OutDir { get; set; }

        private string DateDir {get;set;}

        private HashSet<string> Counties2Crawl { get; set; }
        private HashSet<string> Cities2Crawl { get; set; }

        private HashSet<string> ZipCodes2Crawl { get; set; }

        private Dictionary<string, string> propertyFiles2Analyze { get; set; }
        private Dictionary<string, string> StatsFiles2Analyze { get; set; }
        #endregion

        public AnalysisHelper( CrawlerInputConfig processCfg, string inDir,string dateDir, string outDir)
        {
            ProcessorCfg = processCfg;
            InDir = inDir;
            OutDir = outDir;
            DateDir = dateDir;

            Cities2Crawl = new HashSet<string>();
            ZipCodes2Crawl = new HashSet<string>();
            Counties2Crawl = new HashSet<string>();

            propertyFiles2Analyze = new Dictionary<string, string>();
            StatsFiles2Analyze = new Dictionary<string, string>();
        }

        
        public bool PrepListOfProperyHtmlPages2Extract()
        {
            bool returnValue = false;
            try
            {
                var stateObject = GeoData.DefaultNation.GetState(ProcessorCfg.State);
                if (stateObject != null)
                {
                    // get cities and zipcodes in Hash set ( look up is faster.. O(1) incase of unique hash keys
                    UpdateHashSetWithCityList(ProcessorCfg.CityList);
                    UpdateHashSetWithZipCodeList(ProcessorCfg.ZipCodeList);

                    // convert counties to cities
                    foreach (var county in ProcessorCfg.CountyList)
                    {
                        var listofCities = stateObject.GetListofCitiesInaCounty(county);
                        UpdateHashSetWithCityList(listofCities);
                    }

                    var dirPath = stateObject.GetDirForPropertyData(InDir, DateDir, ProcessorCfg.JobName);

                    // Typical Prop Filename : wa_auburn_12269_31900-104th-Ave-SE-98092_unit-C104_98092.html

                    /*
                     * 1. Enumurate Directory 
                     * 2. For each file, extract City,Zip Code from filename
                     * 3. Add to list based on allowed city,zip list 
                     * 
                     */
                    var htmlFiles = Directory.EnumerateFiles(dirPath, "*.html", SearchOption.TopDirectoryOnly);

                    if( htmlFiles != null && htmlFiles.Count() > 0)
                    {
                        foreach( var htmlFile in htmlFiles)
                        {
                            var filename = Path.GetFileNameWithoutExtension(htmlFile);
                            if(!string.IsNullOrEmpty(filename))
                            {
                                // let us get tokens to understand the eligibility to extract content. 
                                var tokens = filename.Split("_");
                                var city = tokens[1];
                                var zipcode = tokens[4];

                                bool isPropCrawlable = false;

                                if (!string.IsNullOrEmpty(city) && Cities2Crawl.Contains(city))
                                {
                                    isPropCrawlable = true;
                                }
                                else if (!string.IsNullOrEmpty(zipcode) && ZipCodes2Crawl.Contains(zipcode))
                                {
                                    isPropCrawlable = true;
                                }

                                if (isPropCrawlable)
                                {
                                    var propKey = $"{city}_{zipcode}";
                                    AddPropUrl(propKey, htmlFile);
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Unable to enumurate HTML files in dir:{dirPath}");
                    }

                }
                else
                {
                    Console.WriteLine($"State:{ProcessorCfg.State} not found in the given GeoData");
                }
            }
            catch(Exception ex)
            {

            }
            return returnValue;
        }

        public  bool PrepListOfStatsHtmlPages2Extract()
        {
            bool returnValue = false;

            var stateObject = GeoData.DefaultNation.GetState(ProcessorCfg.State);
            if (stateObject != null)
            {
                UpdateHashSetWithCountyList(ProcessorCfg.CountyList);
                UpdateHashSetWithCityList(ProcessorCfg.CityList);
                UpdateHashSetWithZipCodeList(ProcessorCfg.ZipCodeList);

                var dirPath = stateObject.GetDirForStatsData(InDir, DateDir, ProcessorCfg.JobName);
                // ToDo
                // 1. Enumurate files, read file name and determine county,city or zip code stats file. 
                // 2. Short list them based on input list 
                // ex: file names wa_citystats_redmond.html, wa_countystats_king.html, wa_zipcodestats_98075.html 
                var htmlFiles = Directory.EnumerateFiles(dirPath, "*.html", SearchOption.TopDirectoryOnly);

                if (htmlFiles != null && htmlFiles.Count() > 0)
                {
                    foreach (var htmlFile in htmlFiles)
                    {
                        var filename = Path.GetFileNameWithoutExtension(htmlFile);
                        if (!string.IsNullOrEmpty(filename))
                        {
                            // let us get tokens to understand the eligibility to extract content. 
                            var tokens = filename.Split("_");
                            var state = tokens[1];
                            var type = tokens[1];
                            var valueBasedonType = tokens[2];

                            bool isPropCrawlable = false;

                            switch (type)
                            {
                                case "citystats":
                                    if (!string.IsNullOrEmpty(valueBasedonType) && Cities2Crawl.Contains(valueBasedonType))
                                    {
                                        isPropCrawlable = true;
                                    }
                                    break;
                                case "countystats":
                                    if (!string.IsNullOrEmpty(valueBasedonType) && Counties2Crawl.Contains(valueBasedonType))
                                    {
                                        isPropCrawlable = true;
                                    }
                                    break;
                                case "zipcodestats":
                                    if (!string.IsNullOrEmpty(valueBasedonType) && ZipCodes2Crawl.Contains(valueBasedonType))
                                    {
                                        isPropCrawlable = true;
                                    }
                                    break;
                            }                            

                            if (isPropCrawlable)
                            {
                                var propKey = $"{type}_{valueBasedonType}";
                                AddStatsUrl(propKey, htmlFile);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Unable to enumurate HTML files in dir:{dirPath}");
                }

                returnValue = true;
            }
            else
            {
                Console.WriteLine($"State:{ProcessorCfg.State} not found in the given GeoData");
            }
            return returnValue;
        }


        public void QueuePropHTMLDU()
        {
            Console.WriteLine(" Queuing property HTML DU");
            var fileSuffix = $"{ProcessorCfg.JobName}_Properties.csv";
            var stateObject = GeoData.DefaultNation.GetState(ProcessorCfg.State);
            var outputCSVfilePath = stateObject.GetFilePathForAnalysisData(OutDir, DateDir, ProcessorCfg.JobName, fileSuffix);

            if (stateObject != null && !string.IsNullOrEmpty(outputCSVfilePath))
            {
                foreach (var propDU in propertyFiles2Analyze)
                {
                    Console.WriteLine($" Kicking off Prop DU {propDU.Value}");
                    bool returnValue = CrawlerFramework.QueueExtractProperyDataFromHTML(propDU.Value, outputCSVfilePath);
                    if (!returnValue)
                    {
                        Console.WriteLine($"Unable to Process  {propDU.Value}  append  to  {outputCSVfilePath}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Unable to Get State Object for State:{ProcessorCfg.State}");
            }
        }

        public void QueueStatHTMLDU()
        {
            Console.WriteLine(" Queuing Stat HTML DU");
            var fileSuffix = $"{ProcessorCfg.JobName}_Stats.csv";
            var stateObject = GeoData.DefaultNation.GetState(ProcessorCfg.State);
            var outputCSVfilePath = stateObject.GetFilePathForAnalysisData(OutDir, DateDir, ProcessorCfg.JobName, fileSuffix);

            if (stateObject != null && !string.IsNullOrEmpty(outputCSVfilePath))
            {
                foreach (var statDU in StatsFiles2Analyze)
                {
                    Console.WriteLine($" Kicking off Stats DU {statDU.Value}");
                    bool returnValue = CrawlerFramework.QueueExtractStatsFromHTML(statDU.Value, outputCSVfilePath);
                    if (!returnValue)
                    {
                        Console.WriteLine($"Unable to Process  {statDU.Value}  append  to  {outputCSVfilePath}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Unable to Get State Object for State:{ProcessorCfg.State}");
            }
        }


        #region PRIVATE

        private void UpdateHashSetWithCountyList(List<string> counties)
        {
            if (counties != null && counties.Count() > 0)
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cities"></param>
        private void UpdateHashSetWithCityList(List<string> cities)
        {
            if (cities != null && cities.Count() > 0)
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
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipCodeList"></param>
        private void UpdateHashSetWithZipCodeList(List<string> zipCodeList)
        {
            if (zipCodeList != null && zipCodeList.Count() > 0)
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
        }

        private void AddPropUrl(string key, string url)
        {
            if (!propertyFiles2Analyze.ContainsKey(key))
            {
                propertyFiles2Analyze.Add(key, url);
            }
        }

        private void AddStatsUrl(string key, string url)
        {
            if (!StatsFiles2Analyze.ContainsKey(key))
            {
                StatsFiles2Analyze.Add(key, url);
            }
        }

        #endregion
    }

}


