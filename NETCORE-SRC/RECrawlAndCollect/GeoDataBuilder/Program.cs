using System;
using Microsoft.Extensions.CommandLineUtils;


namespace GeoDataBuilder
{
    class Program
    {
        private const string URLOPTION         = "-u|--urls <value>";
        private const string CFGFILEOPTION     = "-c|--file <value>";
        private const string OUTPUTFILEOPTION  = "-o|--file <value>";

        private const string HELP = "-? | -h | --help";

        private const string VERSIONOPTION = "-v|--version";
        private const string VERSION = "0.1";

        private const string DESC = "App to build State,County,City,ZipCode into proper JSON (common schema)";
        static void Main(string[] args)
        {
            var execName = System.AppDomain.CurrentDomain.FriendlyName;
            var app = new CommandLineApplication();
            app.Name = execName;
            app.Description = DESC;

            #region HELP_VERSION
            app.HelpOption(HELP);
            app.VersionOption(VERSIONOPTION, () =>
            {

                return string.Format($"Version {VERSION}");
            });
            #endregion

            #region OPTIONS_ACTIONS
            var configFileOption = app.Option(CFGFILEOPTION, "Configuration file(CSV)", CommandOptionType.SingleValue);
            var urlsOption       = app.Option(URLOPTION, "URL to Crawl GEO DB", CommandOptionType.SingleValue);
            var outputFileOption = app.Option(OUTPUTFILEOPTION, "Configuration file(CSV)", CommandOptionType.SingleValue);

            #endregion

            #region PROCESSING_CODE
            app.OnExecute(() =>
            {
                bool optionExecuted = false;
                bool optionsChecksPassed = false;
                string outputFile="";
                int returnVal = 0;

                if (outputFileOption.HasValue())
                {
                    outputFile = outputFileOption.Value();
                    optionsChecksPassed= !string.IsNullOrEmpty(outputFile);
                }
                else
                {
                    Console.WriteLine("Mandatory param  for output file name  missing");
                    Console.WriteLine($" HELP: {execName} {HELP}");
                    returnVal = -1;
                }

                if (optionsChecksPassed)
                {

                    if (urlsOption.HasValue())
                    {
                        // Let us crawl thoses URLs.
                        var urls = urlsOption.Values;
                        // TBD Crawl and write JSON data
                        optionExecuted = true;
                    }

                    if (configFileOption.HasValue())
                    {
                        var configFile = configFileOption.Value();
                        Console.WriteLine($"Parsing file {configFile} for Geo Data nad output will be written to {outputFile}");
                        var geoDataProcessor = new ProcessGeoDataFromCSV(configFile, outputFile);
                        if( geoDataProcessor.Process())
                        {
                            Console.WriteLine($"Processed data sucessfully, please see outputfile {outputFile}");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to process {configFile}");
                            returnVal = -1;
                        }
                        optionExecuted = true;

                    }
                }

                if (!optionExecuted)
                {
                    Console.WriteLine($" OPT-HELP: {execName} {HELP}");
                    returnVal = -1;
                }
                return returnVal;
            });

            #endregion

            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine($" HELP: {execName} {HELP}");
                }
                else
                {
                    // Setup as needed
                    var result = app.Execute(args);
                    //Console.WriteLine($"Result of {execName} Execute .. {result}");
                }
            }
            catch (CommandParsingException)
            {
                Console.WriteLine($" HELP: {execName} {HELP}");
            }
        }
    }
}
