using System;
using Microsoft.Extensions.CommandLineUtils;
using DataModels;

namespace GenerateConfigTemplate
{
    class Program
    {
        private const string CFGFILEOPTION = "-c|--file <value>";
        private const string OUTPUTFILEOPTION = "-d|--dir <value>";

        private const string HELP = "-? | -h | --help";

        private const string VERSIONOPTION = "-v|--version";
        private const string VERSION = "0.1";

        private const string DESC = "App to create template configuration files for criteria building";
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
            var outputDirOption  = app.Option(OUTPUTFILEOPTION, "OutPut DIR", CommandOptionType.SingleValue);

            #endregion

            #region PROCESSING_CODE
            app.OnExecute(() =>
            {
                string outputFile = "";
                int returnVal = 0;
                if (configFileOption.HasValue())
                {
                    var configFile = configFileOption.Value();
                    Console.WriteLine($"Parsing file {configFile} for Geo Data nad output will be written to {outputFile}");
                    if(!string.IsNullOrEmpty(configFile))
                    {
                        // Let us load it up and generate teamplate configurations. 
                        if ( GeoData.LoadGeoDBFromJsonFile(configFile))
                        {
                            // Let us generate Configuration files.
                            GeoData.GenerateTemplateConfigurationFiles("test", "testDataDir");
                        }
                        else
                        {
                            Console.WriteLine($"Unable to read config file: { configFile}");
                            returnVal = -1;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Configuration fil name is null or empty");
                        returnVal = -1;
                    }

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
                    app.Execute(args);
                }
            }
            catch (CommandParsingException)
            {
                Console.WriteLine($" HELP: {execName} {HELP}");
            }
        }
    }
}
