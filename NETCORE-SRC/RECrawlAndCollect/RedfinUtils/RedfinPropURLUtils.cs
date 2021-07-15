using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedfinUtils
{
    public class RedfinPropURLUtils
    {
        // Prop URL https://www.redfin.com/WA/Moses-Lake/7883-Dune-Lake-Rd-SE-98837/home/16825240
        // https://www.redfin.com/<State>/<City>/7883-Dune-Lake-Rd-SE-<ZipCode>/home/<RefinID>
        // https://www.redfin.com/WA/Issaquah/29398-SE-64th-St-98027/unit-D/home/55520085

        public const string CITY    = "City";
        public const string STATE   = "State";
        public const string ZIPCODE = "ZipCode";
        public const string PROPID  = "PropID";
        public const string ADDRESS = "ADDRESS";
        public const string COUNTY  = "County";
        public const int SEGMENTS_IN_PROP_URL = 6;
        public const int SEGMENTS_IN_COUNTY_STATS_URL = 6;
        public const int SEGMENTS_IN_CITY_STATS_URL = 6;
        public const int SEGMENTS_IN_ZIPCODE_STATS_URL = 4;
        public static Dictionary<string,string> ParseRedfinPropURL(string url)
        {
            var returnValue = new Dictionary<string, string>();
            Uri uri = new Uri(url);
            try
            {
                if (uri != null &&  uri.Segments != null && uri.Segments.Length >= SEGMENTS_IN_PROP_URL)
                {
                    // segment 0 is always "/", segment 1 State , Segment 2  is "city"
                    var state = uri.Segments[1].TrimEnd('/').Trim().ToLower();
                    returnValue.Add(STATE, state);

                    var city    = uri.Segments[2].TrimEnd('/').Trim().ToLower();
                    returnValue.Add(CITY, city);
                    var address = uri.Segments[3].TrimEnd('/').Trim();
                    
                    var tokens = address.Split('-');
                    if( tokens.Length > 0)
                    {
                        var zipCode = tokens[tokens.Length - 1].Trim().ToLower(); ;
                        if(!string.IsNullOrEmpty(zipCode))
                        {
                            returnValue.Add(ZIPCODE, zipCode);
                        }
                    }
                    string propId = "";

                    if(uri.Segments.Length == SEGMENTS_IN_PROP_URL)
                    {
                      propId = uri.Segments[5].Trim();
                    }
                    else
                    {
                        var adderssUnit = uri.Segments[4].TrimEnd('/').Trim();
                        address = $"{address}_{adderssUnit}";
                        propId = uri.Segments[6].Trim();
                    }
                    returnValue.Add(ADDRESS, address);
                    returnValue.Add(PROPID, propId);
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"Failed to Prase URL:{url}, Exception: {ex.Message}");
            }

            return returnValue;
        }

        // Sample URL:  https://www.redfin.com/county/2/WA/Snohomish-County/housing-market
        public static Dictionary<string, string> ParseRedfinCountyStatURL(string url)
        {
            var returnValue = new Dictionary<string, string>();
            Uri uri = new Uri(url);
            try
            {
                if (uri != null && uri.Segments != null && uri.Segments.Length >= SEGMENTS_IN_COUNTY_STATS_URL)
                {
                    // segment 0 is always "/", segment 1 State , Segment 2  is "city"
                    var state = uri.Segments[3].TrimEnd('/').Trim().ToLower();
                    returnValue.Add(STATE, state);

                    var rawCountyStr = uri.Segments[4].TrimEnd('/').Trim().ToLower();
                    var county = rawCountyStr.Replace("-county", "").Trim();
                    returnValue.Add(COUNTY, county);
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to Prase URL:{url}, Exception: {ex.Message}");
            }

            return returnValue;
        }

        // Sample URL:  https://www.redfin.com/city/13/WA/Aberdeen/housing-market
        public static Dictionary<string, string> ParseRedfinCityStatURL(string url)
        {
            var returnValue = new Dictionary<string, string>();
            Uri uri = new Uri(url);
            try
            {
                if (uri != null && uri.Segments != null && uri.Segments.Length >= SEGMENTS_IN_CITY_STATS_URL)
                {
                    // segment 0 is always "/", segment 1 State , Segment 2  is "city"
                    var state = uri.Segments[3].TrimEnd('/').Trim().ToLower();
                    returnValue.Add(STATE, state);

                    var city = uri.Segments[4].TrimEnd('/').Trim().ToLower();                    
                    returnValue.Add(CITY, city);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to Prase URL:{url}, Exception: {ex.Message}");
            }

            return returnValue;
        }

        // Sample URL : https://www.redfin.com/zipcode/98001/housing-market

        public static Dictionary<string, string> ParseRedfinZipcodeStatURL(string url)
        {
            var returnValue = new Dictionary<string, string>();
            Uri uri = new Uri(url);
            try
            {
                if (uri != null && uri.Segments != null && uri.Segments.Length >= SEGMENTS_IN_ZIPCODE_STATS_URL)
                {
                    var zipcode = uri.Segments[2].TrimEnd('/').Trim().ToLower();
                    returnValue.Add(ZIPCODE, zipcode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to Prase URL:{url}, Exception: {ex.Message}");
            }

            return returnValue;
        }



    }
}
