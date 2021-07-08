using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataModels
{
    public class City
    {
        [JsonProperty(PropertyName = "ZipCode-Data")]
        public Dictionary<string, ZipCode> ZipCodeList = new Dictionary<string, ZipCode>();
        [JsonProperty(PropertyName = "CityName")]
        public string Name { get; set; }

        public City() { }
        public City(string name)
        {
            Name = name;
        }

        public bool InsertGeoRecord(string zipcode)
        {
            bool returnValue = false;

            try
            {
                ZipCode zipObject;

                if (!ZipCodeList.TryGetValue(zipcode, out zipObject))
                {
                    zipObject = new ZipCode(zipcode);
                    ZipCodeList[zipcode] = zipObject;
                }
                returnValue = true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to insert geoRecord {zipcode}");
            }

            return returnValue;

        }
    }
}
