using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace DataModels
{
    public class GeoData
    {
        private const string DEFAULT_NATION = "USA";
        private Dictionary<string, Nation> Nations = new Dictionary<string, Nation>();
        private static GeoData _instance = null;

        #region PRIVATE_HELPERS
        private GeoData()
        {
        }
        private static GeoData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GeoData();
                }
                return _instance;
            }
        }

        private Nation GetNationByCode(string code)
        {
            Nation nation = null;

            if (!string.IsNullOrEmpty(code))
            {
                if (!Nations.TryGetValue(code, out nation))
                {
                    nation = new Nation(code);
                    Nations.Add(code, nation);
                }
            }
            return nation;
        }

        private void UpsertNationData(string code, Nation nation)
        {
            if (!string.IsNullOrEmpty(code))
            {
                Nations[code] = nation;
            }
        }
        #endregion

        #region PUBLIC_METHODS
        // Default Nation is USA. 
        public static Nation DefaultNation => GetNation();
        public static Nation GetNation(string code = DEFAULT_NATION)
        {
            if (!string.IsNullOrEmpty(code))
            {
                return Instance?.GetNationByCode(code);
            }
            return null;
        }

        public static bool InsertGeoRecord(string state, string county, string city, string zipcode)
        {
            return DefaultNation.InsertGeoRecord(state, county, city, zipcode);
        }

        public static bool GenerateTemplateConfigurationFiles(string prefix, string dir)
        {
            return DefaultNation.GenerateCriteriaConfigurationTemplates(prefix, dir);
        }

        public static void WriteGeoDBToJsonToFile(string outputFile)
        {
            try
            {
                using (StreamWriter file = File.CreateText(outputFile))
                {
                    //string jsonPayload = JsonConvert.SerializeObject(DefaultNation,Formatting.Indented);
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, DefaultNation);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($" Failed to Write to file {outputFile}, exception: {ex.Message}");
            }
        }

        public static bool LoadGeoDBFromJsonFile(string jsonFileName)
        {
            bool returnValue = false;
            try
            {
                using (StreamReader file = File.OpenText(jsonFileName))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Nation nationData = (Nation)serializer.Deserialize(file, typeof(Nation));
                    if (nationData != null)
                    {
                        Instance.UpsertNationData(DEFAULT_NATION, nationData);
                        returnValue = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Failed to Write to file {jsonFileName}, exception: {ex.Message}");
            }
            return returnValue;
        }

        #endregion
    }
}
