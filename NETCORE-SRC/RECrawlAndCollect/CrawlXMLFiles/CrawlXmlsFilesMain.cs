using System;
using Microsoft.Extensions.CommandLineUtils;
using DataModels;
using CrawlerLib;

namespace CrawlXMLFiles
{
    class CrawlXmlsFilesMain
    {

        private const string STATEOPTION = "-s|--state <value>"; // "ALL" for all states
        private const string ALLSTATES = "all";
        private const string GEODBFILE = "-g|--geodb <value>";
        private const string OUTPUTDIROPTION = "-d|--dir <value>";

        private const string CRAWL_TYPE = "-t|--type <value>";  // posiisble value LIST or STATS or ALL
        public const string CRAWL_ALL = "all";
        public const string CRAWL_LIST = "list";
        public const string CRAWL_STATS = "stats";

        private const string HELP = "-? | -h | --help";

        private const string VERSIONOPTION = "-v|--version";
        private const string VERSION = "0.1";

        private const string DESC = "App to Crawl XMl Files(sitemaps and xml file with prop links) from Redfin and store them";
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
            var stateCodesOption = app.Option(STATEOPTION, "StateCode", CommandOptionType.MultipleValue);
            var CrawlTypeOption  = app.Option(CRAWL_TYPE, "CrawlType Option", CommandOptionType.SingleValue);
            var outputDirOption  = app.Option(OUTPUTDIROPTION, "OutPut DIR", CommandOptionType.SingleValue);
            var geoCfgFileOption = app.Option(GEODBFILE, "geo Config file", CommandOptionType.SingleValue);

            #endregion

            #region PROCESSING_CODE
            app.OnExecute(() =>
            {                
                int returnVal = 0;
                string outputDir = ".";

                if (geoCfgFileOption.HasValue() && stateCodesOption.HasValue())
                {
                    var geoCfgFile = geoCfgFileOption.Value();
                    if( outputDirOption.HasValue() && !string.IsNullOrEmpty(outputDirOption.Value())) 
                    {
                        outputDir = outputDirOption.Value();
                    }                 

                    if (!string.IsNullOrEmpty(geoCfgFile))
                    {
                        var stateCodes = stateCodesOption.Values;
                        if (stateCodes != null && stateCodes.Count > 0)
                        {
                            // Let us load it up and generate teamplate configurations. 
                            if (GeoData.LoadGeoDBFromJsonFile(geoCfgFile))
                            {
                                bool allStates = false;
                                var crawlXmlAndSave = new CrawlXmlandSaveInFile(outputDir);
                                //  Let Start Crawling  for XML Files.
                                foreach ( var stateCode in stateCodes)
                                {
                                    // State codes are always given in upper.
                                    if( stateCode.Trim().ToLower() == ALLSTATES)
                                    {
                                        allStates = true;
                                    }
                                }

                                string crawlType = CRAWL_ALL;
                                if (CrawlTypeOption.HasValue())
                                {
                                    crawlType = CrawlTypeOption.Value()?.Trim().ToLower();
                                }

                                if(allStates)
                                {
                                    // call for all states
                                    if(!crawlXmlAndSave.CrawlAllStates(crawlType))
                                    {
                                        Console.WriteLine($"Failed to Crawl All States");
                                    }
                                }
                                else
                                {
                                    foreach (var stateCode in stateCodes)
                                    {
                                        if(!crawlXmlAndSave.CrawlSpecificState(stateCode, crawlType))
                                        {
                                            Console.WriteLine($"Failed to Crawl {stateCode}");
                                        }
                                    }
                                }
                                // Queued all the work, let us kick off Threads to do their job. 
                                CrawlerFramework.KickoffJobAgents();
                                // Wait for all the work to be finished by Threads.
                                CrawlerFramework.WaitForAllJobstoComplete();
                                Console.WriteLine(" All Jobs in queue processed, hence exiting");
                            }
                            else
                            {
                                Console.WriteLine($"Unable to read config file: { geoCfgFile}");
                                returnVal = -1;
                            }

                        }
                    }
                    else
                    {
                        Console.WriteLine($"Empty Configuration file name for -g option : {geoCfgFile}");
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
