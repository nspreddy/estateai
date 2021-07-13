using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataModels
{
    public class CrawlerInputConfig
    {
        [JsonProperty(PropertyName = "JobName")]
        public string JobName { get; set; }

        [JsonProperty(PropertyName = "County-List")]
        public List<string> CountyList { get; set; }

        [JsonProperty(PropertyName = "City-List")]
        public List<string> CityList { get; set; }

        [JsonProperty(PropertyName = "ZipCode-List")]
        public List<string> ZipCodeList { get; set;}

        public CrawlerInputConfig()
        {
            CountyList = new List<string>();
            CityList = new List<string>();
            ZipCodeList = new List<string>();
        }
    }
}
