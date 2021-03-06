﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CommonAndUtils;
using System.IO;

namespace DataModels
{
    public class City
    {
        [JsonProperty(PropertyName = "ZipCode-Data")]
        public Dictionary<string, ZipCode> ZipCodeList = new Dictionary<string, ZipCode>();
        [JsonProperty(PropertyName = "CityName")]
        public string Name { get; set; }

        [JsonIgnore]
        private County Parent { get;set;}

        public City() { }
        public City(string name, County county)
        {
            Name = name;
            Parent = county;
        }
        public void ValidateParent(County county)
        {
            if( Parent == null)
            {
                Parent = county;
            }

            foreach( var kv in ZipCodeList)
            {
                kv.Value?.ValidateParent(this);
            }
        }

        public bool InsertGeoRecord(string zipcode)
        {
            bool returnValue = false;

            try
            {
                ZipCode zipObject;

                if (!ZipCodeList.TryGetValue(zipcode, out zipObject))
                {
                    zipObject = new ZipCode(zipcode,this);
                    ZipCodeList[zipcode] = zipObject;
                }
                returnValue = true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to insert geoRecord {zipcode} exception :{e.Message}");
            }

            return returnValue;

        }

        public bool GenerateCriteriaConfigurationTemplates(string state,string fileprefix, string dir)
        {
            bool returnValue = false;

            try
            {
                // Let us generate Criteria for City
                var filename = $"{ fileprefix}.json";
                GenerateConfigTemplateCityScoped(state, dir, filename);
                // Let us create a direcrtory strcture .. <dir>/<county>/<state>/<county>/<city>/<zipcode>               
                var computedDir = Path.Combine(dir, Name);


                if (ZipCodeList.Count > 0)
                {
                    foreach (var kv in ZipCodeList)
                    {
                        var zipcode = kv.Key;
                        var zipcodeObject = kv.Value;
                        Console.WriteLine($"Calling County  {zipcode} to create configuration");
                        var prefix = $"{fileprefix}_{zipcode}";
                        zipcodeObject.GenerateCriteriaConfigurationTemplates(state,prefix, computedDir);
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

        #region PRIVATE_HELPERS
        private void GenerateConfigTemplateCityScoped(string state, string dir, string filename)
        {
            try
            {
                var processCfg = new ProcessorConfig();
                var jobName = $"Job_{state}_City_{Name}";
                processCfg.GenerateRandomData(state, Criteria.SCOPE_CITY, Name,jobName);

                // Write to JSON File. 
                string jsonPayload = JsonConvert.SerializeObject(processCfg, Formatting.Indented);
                FileReadWriteUtil.WriteToFile(dir, filename, jsonPayload);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception  While Writing County Configuration to JSON file {ex.Message}");
            }
        }

        #endregion

    }
}
