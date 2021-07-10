using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CommonAndUtils;

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

        public bool GenerateCriteriaConfigurationTemplates(string state,string fileprefix,string dir)
        {
            bool returnValue = false;

            try
            {
                // Let us generate Criteria for ZipCode
                var filename = $"{ fileprefix}.json";
                GenerateConfigTemplateZipScoped(state,dir,filename);
                returnValue = true;                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception  While genrating config templates(Nation Object) {ex.Message}");
            }

            return returnValue;
        }

        #region PRIVATE_HELPERS
        private void GenerateConfigTemplateZipScoped(string state, string dir, string filename)
        {
            try
            {
                var crawlCfg = new CrawlConfig();
                crawlCfg.GenerateRandomData(state, Criteria.SCOPE_ZIP, Name);

                // Write to JSON File. 
                string jsonPayload = JsonConvert.SerializeObject(crawlCfg, Formatting.Indented);
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
