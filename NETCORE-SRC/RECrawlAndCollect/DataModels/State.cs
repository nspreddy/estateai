using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace DataModels
{
    public class State
    {
        [JsonProperty(PropertyName = "County-Data")]
        public Dictionary<string,County> CountyList = new Dictionary<string,County>();
        [JsonProperty(PropertyName = "StateCode")]
        public string Name { get; set; }

        public State() { }
        public State( string name)
        {
            Name = name;
        }

        public bool InsertGeoRecord( string county, string city, string zipcode)
        {
            bool returnValue = false;

            try
            {
                County countyObject;

                if (!CountyList.TryGetValue(county, out countyObject))
                {
                    countyObject = new County(county);
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
                    foreach (var kv in CountyList)
                    {
                        var county = kv.Key;
                        var countyObject = kv.Value;
                        Console.WriteLine($"Calling County  {county} to create configuration");
                        var prefix = $"{fileprefix}_{county}";
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

    }
}
