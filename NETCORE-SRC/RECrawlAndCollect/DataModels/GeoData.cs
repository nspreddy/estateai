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

        private Nation  GetNation( string code)
        {
            Nation nation = null;

            if(!string.IsNullOrEmpty(code))
            {
                if( !Nations.TryGetValue(code, out nation))
                {
                    nation = new Nation();
                    Nations.Add(code, nation);
                }
            }
            return nation;
        }
        // Default Nation is USA. 
        private static Nation DefaultNation => Instance?.GetNation(DEFAULT_NATION);

        public static bool InsertGeoRecord(string state, string county, string city, string zipcode)
        {  
            if( !string.IsNullOrEmpty(state) && !string.IsNullOrEmpty(county) && !string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(zipcode))
            {
                return DefaultNation.InsertGeoRecord(state, county, city, zipcode);
            }
            
            return false;            
        }

        public static void WriteJsonToFile( string outputFile)
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
    }
}
