using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataModels
{
    public class Criteria
    {
        [JsonProperty(PropertyName = "State")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "ScopeType")]
        public string ScopeType { get; set; }

        [JsonProperty(PropertyName = "ScopeName")]
        public string ScopeName { get; set; }

        [JsonProperty(PropertyName ="PropertyType")]
        public string PropertyType{ get; set; }

        [JsonProperty(PropertyName = "WalkScore")]
        public int WalkScore { get; set; }

        [JsonProperty(PropertyName = "CommuteScore")]
        public int CommuteScore { get; set; }

        [JsonProperty(PropertyName = "TransitScore")]
        public int TransitScore { get; set; }

        [JsonProperty(PropertyName = "HOAMax")]
        public int HOAMax { get; set; }

        [JsonProperty(PropertyName = "SqftMin")]
        public int SqftMin { get; set; }

        [JsonProperty(PropertyName = "MinBeds")]
        public int MinBeds { get; set; }

        [JsonProperty(PropertyName = "MinaBaths")]
        public int MinBaths { get; set; }

        [JsonProperty(PropertyName = "ParkingSpots")]
        public int ParkingSpots { get; set; }

        [JsonProperty(PropertyName = "YearBuilt")]
        public int YearBuilt { get; set; }
    }
}
