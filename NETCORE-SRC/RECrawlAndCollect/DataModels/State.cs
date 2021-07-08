using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                Console.WriteLine($"Unable to insert geoRecord {county}, {city},{zipcode}");
            }

            return returnValue;

        }
    }
}
