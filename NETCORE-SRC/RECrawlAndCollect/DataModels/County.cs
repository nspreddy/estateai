using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
                Console.WriteLine($"Unable to insert geoRecord {city},{zipcode}");
            }

            return returnValue;

        }
    }
}
