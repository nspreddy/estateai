using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

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
                if (!string.IsNullOrEmpty(state) && !string.IsNullOrEmpty(county) && !string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(zipcode))
                {
                    State stateObject;
                    if (!StatesList.TryGetValue(state, out stateObject))
                    {
                        stateObject = new State(state);
                        StatesList[state] = stateObject;
                    }
                    returnValue = stateObject.InsertGeoRecord(county, city, zipcode);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Unable to insert geoRecord {state}, {county}, {city},{zipcode} exception :{e.Message}");
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

                if (StatesList.Count > 0)
                {
                    foreach (var kv in StatesList)
                    {
                        var state = kv.Key;
                        var stateObject = kv.Value;
                        Console.WriteLine($"Calling State {state} to create configuration");
                        var prefix = $"{fileprefix}_{state}";
                        stateObject.GenerateCriteriaConfigurationTemplates(prefix, computedDir);
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
