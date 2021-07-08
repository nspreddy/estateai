using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataModels
{
    public class Nation
    {
        [JsonProperty(PropertyName = "States-Data")]
        public Dictionary<string, State> StatesList = new Dictionary<string, State>();
        [JsonProperty(PropertyName = "Nation")]
        public string Name { get; set; }

        public Nation()
        {
        }

        public Nation(string name)
        {
            Name = name;
        }

        public bool InsertGeoRecord(string state, string county, string city, string zipcode)
        {
            bool returnValue = false;

            try
            {
                State stateObject;
                
                if( !StatesList.TryGetValue(state,out stateObject))
                {
                    stateObject= new State(state);
                    StatesList[state] = stateObject;
                }
                returnValue = stateObject.InsertGeoRecord(county, city, zipcode);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Unable to insert geoRecord {state}, {county}, {city},{zipcode}");
            }

            return returnValue;

        }

    }
}
