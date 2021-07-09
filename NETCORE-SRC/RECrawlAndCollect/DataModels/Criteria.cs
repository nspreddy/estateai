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
        public const string SCOPE_COUNTY = "County";
        public const string SCOPE_CITY   = "City";
        public const string SCOPE_ZIP    = "Zip";

        public const string PROP_SFR   = "SFR";
        public const string PROP_TH    = "TOWNHOME";
        public const string PROP_CONDO = "CONDO";
        public const string PROP_LAND  = "LAND";



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

        public Criteria()
        {

        }
        public void GenerateRandomData( string state, string scopetype, string scopename,string propType)
        {
            var rnd = new Random();
            State = state;
            ScopeType = scopetype;
            ScopeName = scopename;
            PropertyType = propType;
            WalkScore = rnd.Next(0, 100);
            CommuteScore = rnd.Next(0, 100);
            TransitScore = rnd.Next(0, 100);
            HOAMax = rnd.Next(0, 350);
            SqftMin = rnd.Next(0, 1000);
            MinBeds = rnd.Next(2, 5);
            MinBaths = rnd.Next(2, 4);
            ParkingSpots = rnd.Next(1,3);
            YearBuilt = rnd.Next(2000, 2022);
        }
    }
}
