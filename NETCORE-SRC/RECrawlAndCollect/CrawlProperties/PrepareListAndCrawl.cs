using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModels;
using CommonAndUtils;

namespace CrawlProperties
{
    internal class PrepareListAndCrawl
    {
        private CrawlerInputConfig crawlerConfig { get; set; }
        private string XmlInDir { get; set; }
        private string XmlOutDir { get; set; }

        private HashSet<string> Cities2Crawl { get; set; }

        private HashSet<string> PropertyUrls2Crawl { get; set; }

        public PrepareListAndCrawl(CrawlerInputConfig crawlConfig, string inDir, string outDir)
        {
            crawlerConfig= crawlConfig;
            XmlInDir  = inDir;
            XmlOutDir = outDir;
            Cities2Crawl = new HashSet<string>();
        }

        public bool PrepareList2Crawl()
        {
            bool returnValue = false;

            var stateObject = GeoData.DefaultNation.GetState(crawlerConfig.State);
            if (stateObject != null)
            {
                // convert counties to cities
                UpdateHashSetWithCityList(crawlerConfig.CityList);
                foreach( var county in crawlerConfig.CountyList)
                {
                    var listofCities = stateObject.GetListofCitiesInaCounty(county);
                    UpdateHashSetWithCityList(listofCities);
                }
                Console.WriteLine($"Cities total: {Cities2Crawl.Count()} read to Crawl");
                // Let us get XML file path with URLs of properties to CRAWL.
                var xmlPropUrlFile = stateObject.GetFilePathWithRelativeDirPath(XmlInDir, State.SALE_LISTINGS_PREFIX);
                if (xmlPropUrlFile != null)
                {

                }

            }
            else
            {
                Console.WriteLine($"State:{crawlerConfig.State} not found in the given GeoData");
            }


            return returnValue;
        }

        private void  UpdateHashSetWithCityList( List<string> cities)
        {
            foreach( var city in cities)
            {
                if ( !Cities2Crawl.Contains(city)){
                    Cities2Crawl.Add(city);
                }
            }
        }

        private void AddPropUrl( string url)
        {
            if(!PropertyUrls2Crawl.Contains(url))
            {
                PropertyUrls2Crawl.Add(url);
            }
        }

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
                            Console.WriteLine($"Prop-Link: {propLinkUrl}");
                            // ToDo: Let us make sure the prop link City is in CityList
                            // ToDo: if not ZipCodeList
                            // ToDo: if present then Addprop2Crawl
                            AddPropUrl(propLinkUrl);
                        }
                        
                    }
                }

            }
            catch(Exception ex)
            {

            }

            return returnValue;
        }

        
    }
}
