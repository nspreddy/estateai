using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataModels
{
    public class CrawlConfig
    {
        [JsonProperty(PropertyName ="geoCfg")]
        public string geoCfgFile { get; set;}

        [JsonProperty(PropertyName = "Critera-Data")]
        public List<Criteria> criterias { get; set; }
    }
}
