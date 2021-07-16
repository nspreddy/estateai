using System;
using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;
using DataModels;
using CrawlerLib;

namespace CrawlProperties
{
    class CrawlPropsAndStats
    {
        
        private const string GEOCFGFILEOPTION   = "-g|--geodb <value>";
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

            #region PROCESSING_CODE
            app.Command(CrawlHelper.CRAWL_PROPS_CMD, command =>
            {
                var geoconfigFileOption   = command.Option(GEOCFGFILEOPTION, "GeoDB Configfile", CommandOptionType.SingleValue);
                var outputDirOption       = command.Option(OUTPUTDIROPTION, "OutPut DIR", CommandOptionType.SingleValue);
                var inputDirOption        = command.Option(INPUTDIROPTION, "Input DIR", CommandOptionType.SingleValue);
                var crawlConfigOption     = command.Option(CRAWLCONFIG, "Crawl Config File", CommandOptionType.MultipleValue);
                
                command.OnExecute(() =>
                {
                    var geoConfigFile = geoconfigFileOption.Value();
                    var dirToSave = outputDirOption.Value();
                    var dirToReadXmlFiles = inputDirOption.Value();
                    var crawlConfigFiles = crawlConfigOption.Values;

                    var returnValue= ProcessCrawlCommand(CrawlHelper.CRAWL_PROPS_CMD, geoConfigFile, dirToSave, dirToReadXmlFiles, crawlConfigFiles);
                    if( returnValue == -1)
                    {
                        Console.WriteLine($" HELP: {execName} {HELP}");
                    }
                    return returnValue;
                });
            });

            app.Command(CrawlHelper.CRAWL_STATS_CMD, command =>
            {
                var geoconfigFileOption = command.Option(GEOCFGFILEOPTION, "GeoDB Configfile", CommandOptionType.SingleValue);
                var outputDirOption     = command.Option(OUTPUTDIROPTION, "OutPut DIR", CommandOptionType.SingleValue);
                var inputDirOption      = command.Option(INPUTDIROPTION, "Input DIR", CommandOptionType.SingleValue);
                var crawlConfigOption   = command.Option(CRAWLCONFIG, "Crawl Config File", CommandOptionType.MultipleValue);

                command.OnExecute(() =>
                {
                    var geoConfigFile = geoconfigFileOption.Value();
                    var dirToSave = outputDirOption.Value();
                    var dirToReadXmlFiles = inputDirOption.Value();
                    var crawlConfigFiles = crawlConfigOption.Values;

                    var returnValue = ProcessCrawlCommand(CrawlHelper.CRAWL_STATS_CMD, geoConfigFile, dirToSave, dirToReadXmlFiles, crawlConfigFiles);
                    if (returnValue == -1)
                    {
                        Console.WriteLine($" HELP: {execName} {HELP}");
                    }
                    return returnValue;
                });
            });


            app.OnExecute(() =>
            {
                Console.WriteLine($" HELP: {execName} {HELP}");
                return 0;
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


        private static int ProcessCrawlCommand(string command, string geoConfigFile, string dirToSave, string dirToReadXmlFiles, List<string> crawlConfigFiles)
        {
            int returnVal = -1;

            if (!string.IsNullOrEmpty(command) &&
                !string.IsNullOrEmpty(geoConfigFile) &&
                !string.IsNullOrEmpty(dirToReadXmlFiles) &&
                crawlConfigFiles != null &&
                crawlConfigFiles.Count > 0)
            {
                if (string.IsNullOrEmpty(dirToSave))
                {
                    dirToSave = ".";
                }
                Console.WriteLine($"Parsing file {geoConfigFile} for Geo Data nad output will be written to Directory {dirToSave}");
                if (GeoData.LoadGeoDBFromJsonFile(geoConfigFile))
                {
                    if (crawlConfigFiles != null && crawlConfigFiles.Count > 0)
                    {
                        foreach (var crawlConfig in crawlConfigFiles)
                        {
                            switch (command.ToLower())
                            {
                                case CrawlHelper.CRAWL_PROPS_CMD:
                                case CrawlHelper.CRAWL_STATS_CMD:
                                    ProcessCrawlConfigAndStartCrawling(command,crawlConfig, dirToReadXmlFiles, dirToSave);
                                    break;                                
                                default:
                                    Console.WriteLine($"Command not understood {command}");
                                    break;
                            }
                            
                        }
                        // Wait for Crawler to finish procssing all the Jobs.
                        // Queued all the work, let us kick off Threads to do their job. 
                        CrawlerFramework.KickoffJobAgents();
                        // Wait for all the work to be finished by Threads.
                        CrawlerFramework.WaitForAllJobstoComplete();
                        Console.WriteLine(" All Jobs in queue processed, hence exiting");
                        returnVal = 0;
                    }
                    else
                    {
                        Console.WriteLine($"CrawlConfiguration File missing");
                    }
                }
                else
                {
                    Console.WriteLine($"Unable to read geoDB  file: {geoConfigFile}");
                }
            }
            else
            {
                Console.WriteLine($" Error in input params: CMD:{command}, geoDB:{geoConfigFile}, XMlInputFilesDir:{dirToReadXmlFiles}");
            }            
            return returnVal;
        }

        private static bool ProcessCrawlConfigAndStartCrawling( string command, string configFile,  string inputDir, string outputDir)
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
                        var crawlHelper = new CrawlHelper(command,crawlConfig,inputDir,outputDir);
                        if(crawlHelper.PrepareList2Crawl())
                        {
                            // Now we have a list, let us crawl.
                            // ToDo.. initiate crawler. 
                            crawlHelper.QueueCrawlJobs();
                            returnValue = true;
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
