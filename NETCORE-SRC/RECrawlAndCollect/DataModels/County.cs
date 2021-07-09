using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using CommonAndUtils;
using System.IO;

namespace DataModels
{
    public class County
    {
        [JsonProperty(PropertyName = "City-Data")]
        public Dictionary<string, City> CityList = new Dictionary<string, City>();
        [JsonProperty(PropertyName = "CountyName")]
        public string Name { get; set; }

        public County() { }
        public County(string name)
        {
            Name = name;
        }

        public bool InsertGeoRecord( string city, string zipcode)
        {
            bool returnValue = false;

            try
            {
                City cityObject;

                if (!CityList.TryGetValue(city, out cityObject))
                {
                    cityObject = new City(city);
                    CityList[city] = cityObject;
                }
                returnValue = cityObject.InsertGeoRecord(zipcode);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to insert geoRecord {city},{zipcode}, exception :{e.Message}");
            }

            return returnValue;
        }

        public bool GenerateCriteriaConfigurationTemplates(string state,string fileprefix,string dir)
        {
            bool returnValue = false;

            try
            {
                // Let us generate Criteria for County, use {fileprefix}.json as filename
                var filename = $"{fileprefix}.json";
                GenerateConfigTemplateCountyScoped(state,dir, filename);

                // Let us create a direcrtory strcture .. <dir>/<county>/<state>/<county>/<city>/<zipcode>               
                var computedDir = Path.Combine(dir, Name);

                if (CityList.Count > 0)
                {
                    foreach (var kv in CityList)
                    {
                        var city = kv.Key;
                        var cityObject = kv.Value;
                        Console.WriteLine($"Calling County  {city} to create configuration");
                        var prefix = $"{fileprefix}_{city}";
                        cityObject.GenerateCriteriaConfigurationTemplates(state,prefix, computedDir);
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

        #region PRIVATE_HELPERS
        private void GenerateConfigTemplateCountyScoped(string state, string dir, string filename)
        {
            try
            {
                var crawlCfg = new CrawlConfig();                
                crawlCfg.GenerateRandomData(state, Criteria.SCOPE_COUNTY, Name);

                // Write to JSON File. 
                string jsonPayload = JsonConvert.SerializeObject(crawlCfg, Formatting.Indented);
                JsonFileReadWriteUtil.WriteJsonToFile(dir, filename, jsonPayload);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception  While Writing County Configuration to JSON file {ex.Message}");
            }
        }

        #endregion

    }
}
