using System;
using Microsoft.Extensions.CommandLineUtils;
using DataModels;

namespace CrawlProperties
{
    class CrawlProps
    {
        private const string CFGFILEOPTION   = "-g|--geodb <value>";
        private const string CRAWLCONFIG     = "-c|--crawl <value>";
        private const string INPUTDIROPTION = "-i|--inputdir <value>";
        private const string OUTPUTDIROPTION = "-d|--outputdir <value>";

        private const string HELP = "-? | -h | --help";

        private const string VERSIONOPTION = "-v|--version";
        private const string VERSION = "0.1";

        private const string DESC = "App to crawl HTML pages and understand the content.";
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
            var outputDirOption = app.Option(OUTPUTDIROPTION, "OutPut DIR", CommandOptionType.SingleValue);
            var inputDirOption = app.Option(INPUTDIROPTION, "Input DIR", CommandOptionType.SingleValue);
            var crawlConfigOption = app.Option(CRAWLCONFIG, "Crawl Config File", CommandOptionType.MultipleValue); ;

            #endregion

            #region PROCESSING_CODE
            app.OnExecute(() =>
            {
                int returnVal = 0;
                if (configFileOption.HasValue())
                {
                    var geoConfigFile = configFileOption.Value();
                    var dirToSave = outputDirOption.Value();
                    var dirToReadXmlFiles = inputDirOption.Value();

                    if (string.IsNullOrEmpty(dirToSave))
                    {
                        dirToSave = ".";
                    }
                    Console.WriteLine($"Parsing file {geoConfigFile} for Geo Data nad output will be written to Directory {dirToSave}");
                    if (!string.IsNullOrEmpty(geoConfigFile) && !string.IsNullOrEmpty(dirToReadXmlFiles) )
                    {
                        // Let us load it up and generate teamplate configurations. 
                        if (GeoData.LoadGeoDBFromJsonFile(geoConfigFile))
                        {
                            if (crawlConfigOption.HasValue()) {
                                var crawlConfigFiles = crawlConfigOption.Values;
                                if( crawlConfigFiles != null)
                                {
                                    foreach(var crawlConfig in crawlConfigFiles)
                                    {
                                        ProcessCrawlConfiguration(crawlConfig,dirToReadXmlFiles,dirToSave);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Configfile configuration is missing");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Config file option is missing");
                            }
                            
                        }
                        else
                        {
                            Console.WriteLine($"Unable to read config file: {geoConfigFile}");
                            returnVal = -1;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Unable to read config file: {geoConfigFile} or Input Dir is: {dirToReadXmlFiles}");
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

        private static bool ProcessCrawlConfiguration( string configFile,  string inputDir, string outputDir)
        {
            bool returnValue = false;

            try
            {
                // Let us get JSON object from configuarion file. 
                var crawlConfig = CrawlerInputConfig.GetCrawlerConfigurationFromJsonFile(configFile);
                if( crawlConfig != null)
                {
                    // let us get the XML File based on State. 
                    var stateObject = GeoData.DefaultNation.GetState(crawlConfig.State);
                    if( stateObject != null)
                    {
                        var proplistAndCrawlHelper = new PrepareListAndCrawl(crawlConfig,inputDir,outputDir);
                        if(proplistAndCrawlHelper.PrepareList2Crawl())
                        {
                            // Now we have a list, let us crawl.
                            // ToDO.. initiate crawler. 
                            if (proplistAndCrawlHelper.StartCrawling())
                            {
                                Console.WriteLine("Sucessfully Crawled..");
                            }
                            else
                            {
                                Console.WriteLine("Failed to Crawl Content from shortlisted URLs");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Failed to prepare prop list to Crawl");
                        }

                    }
                    else
                    {
                        Console.WriteLine($"State:{crawlConfig.State} not found in the given GeoData");
                    }                    
                }
                else
                {
                    Console.WriteLine($"Unable to read Config(JSON): {configFile}");
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Unable to  process config: {configFile}, Exception: {ex.Message}");
            }
            return returnValue;
        }
    }
}
