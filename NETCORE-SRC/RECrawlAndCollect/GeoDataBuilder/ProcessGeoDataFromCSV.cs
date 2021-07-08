using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace GeoDataBuilder
{
    class ProcessGeoDataFromCSV
    {
        private const int NUMBEROFENTITIES = 15;        
        private const int ZIP_COL_INDEX = 0;
        private const int CITY_COL_INDEX = 3;
        private const int STATE_COL_INDEX = 6;
        private const int COUNTY_COL_INDEX = 7;
        private const int COUNTRY_COL_INDEX = 11;
        private string inputFileName;
        private string outputFileName;


        private const string CSV_REGEX = @"(?:,|^)([^"",]+|""(?:[^""]|"""")*"")?";
        

        #region PRIVATE_METHODS

        private bool ProcessRow(string row)
        {
            bool returnValue = false;

            try
            {
                var regExCollections = Regex.Matches(row, CSV_REGEX);
                if( regExCollections != null && regExCollections.Count == NUMBEROFENTITIES)
                {
                    if( regExCollections[0].Value.Trim()== "zip")
                    {
                        Console.WriteLine("Skipping header row");
                    }
                    else
                    {
                        var state = regExCollections[STATE_COL_INDEX]?.Value.TrimStart(',').Trim();
                        var county  = regExCollections[COUNTY_COL_INDEX]?.Value.TrimStart(',').Trim('\"').Trim();
                        var city    = regExCollections[CITY_COL_INDEX]?.Value.TrimStart(',').Trim();
                        var zipcode = regExCollections[ZIP_COL_INDEX]?.Value.TrimStart(',').Trim();

                        Console.WriteLine($" State: {state}, County:{county}, City: {city} , ZipCode: {zipcode}");
                    }                   
                }
                else
                {
                    Console.WriteLine($" Wrong Format Row {row}");
                }               

            }catch (Exception ex)
            {
                Console.WriteLine($"Exception : {ex.Message}");
            }

            return returnValue;
        }

        #endregion


        public ProcessGeoDataFromCSV( string inputfile, string outputfile)
        {
            inputFileName = inputfile;
            outputFileName = outputfile;
        }

        public bool Process()
        {
            bool returnVal = false;

            if (!string.IsNullOrEmpty(inputFileName) && !string.IsNullOrEmpty(outputFileName))
            {
                // Start reading input file .
                try
                {
                    using (StreamReader sr = new StreamReader(inputFileName))
                    {
                        string line;
                        int failedRows=0;
                        // Read and display lines from the file until the end of
                        // the file is reached.
                        while ((line = sr.ReadLine()) != null)
                        {
                            if( !ProcessRow(line))
                            {
                                ++failedRows;                                
                            }
                        }
                        var outputStr =  failedRows > 0 ? $" Failed ROWS: {failedRows}" : $"processed with no errors";
                        Console.WriteLine(outputStr);
                    }

                    returnVal = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception While Processing {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"FileNames null or empty.. I:{inputFileName} O:{outputFileName}");
            }
            

            return returnVal;
        }

       
    }
}
