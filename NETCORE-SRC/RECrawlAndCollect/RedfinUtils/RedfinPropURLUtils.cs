﻿using System;
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

        public const string CITY    = "City";
        public const string STATE   = "State";
        public const string ZIPCODE = "ZipCode";
        public const string PROPID  = "PropID";
        public const int SEGMENTS_IN_PROP_URL = 6;
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
                    // extract ID
                    var propId = uri.Segments[5].Trim();
                    returnValue.Add(PROPID, propId);

                }
            }catch(Exception ex)
            {
                Console.WriteLine($"Failed to Prase URL:{url}, Exception: {ex.Message}");
            }

            return returnValue;
        }
    }
}