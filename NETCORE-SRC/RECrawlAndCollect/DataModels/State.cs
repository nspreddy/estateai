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
    }
}
