using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using DataModels;

namespace GeoDataBuilder
{
    class ProcessGeoDataFromCSV
    {
        private const int NUMBEROFENTITIES = 15;        
        private const int ZIP_COL_INDEX = 0;
        private const int TYPE_COL_INDEX = 1;
        private const int DECOMM_COL_INDEX = 2;
        private const int CITY_COL_INDEX = 3;
        private const int STATE_COL_INDEX = 6;
        private const int COUNTY_COL_INDEX = 7;
        private const int COUNTRY_COL_INDEX = 11;

        private const string PARSING_LOGS = "parsinglogs.txt";

        private string inputFileName;
        private string outputFileName;
        private StreamWriter parsingLogWriter;// for error and warnging


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
                        returnValue = true;
                    }
                    else
                    {
                        var type    = regExCollections[TYPE_COL_INDEX]?.Value.TrimStart(',').Trim();
                        var decomm  = regExCollections[DECOMM_COL_INDEX]?.Value.TrimStart(',').Trim();

                        var state   = regExCollections[STATE_COL_INDEX]?.Value.TrimStart(',').Trim();
                        var county  = regExCollections[COUNTY_COL_INDEX]?.Value.TrimStart(',').Trim('\"').Trim();
                        var city    = regExCollections[CITY_COL_INDEX]?.Value.TrimStart(',').Trim();
                        var zipcode = regExCollections[ZIP_COL_INDEX]?.Value.TrimStart(',').Trim();

                        if( Convert.ToInt32(decomm) == 1)
                        {
                            var infoStr = $"Information - Record Decomm'd ( Type: {type} Decomm: {decomm} State: {state}, County:{county}, City: {city} , ZipCode: {zipcode})";
                            parsingLogWriter.WriteLine(infoStr);
                            returnValue = true;
                        }
                        else
                        {
                            
                            if(string.IsNullOrEmpty(county))
                            {
                                county = type;
                            }
                            returnValue = GeoData.InsertGeoRecord(state, county, city, zipcode);
                            if (!returnValue)
                            {
                                var warnStr = $"Warning - Failed to Insert Record( Type: {type} Decomm: {decomm} State: {state}, County:{county}, City: {city} , ZipCode: {zipcode})";
                                parsingLogWriter.WriteLine(warnStr);
                            }

                        }                        
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
                        parsingLogWriter = File.CreateText(PARSING_LOGS);
                        // Read and display lines from the file until the end of
                        // the file is reached.
                        while ((line = sr.ReadLine()) != null)
                        {
                            if( !ProcessRow(line))
                            {
                                ++failedRows;                                
                            }
                        }
                        var outputStr =  failedRows > 0 ? $"Summary: Failed ROWS: {failedRows}" : $"Summary: Processed with no errors";
                        parsingLogWriter.WriteLine(outputStr);
                        Console.WriteLine(outputStr);
                    }
                    // Time to write File with JSON data
                    GeoData.WriteJsonToFile(outputFileName);
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
