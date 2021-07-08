using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataModels
{
    public class ZipCode
    {
        [JsonProperty(PropertyName = "zipcode")]
        public string Name { get; set; }

        public ZipCode() { }
        public ZipCode(string name)
        {
            Name = name;
        }
    }
}
