﻿using System;
using Microsoft.Extensions.CommandLineUtils;
using DataModels;

namespace CrawlXMLFiles
{
    class CrawlXmlsFilesMain
    {
        private const string STATEOPTION = "-s|--state <value>"; // "ALL" for all states
        private const string ALLSTATES = "ALL";
        private const string GEODBFILE = "-g|--geodb <value>";
        private const string OUTPUTDIROPTION = "-d|--dir <value>";

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
            var stateCodesOption = app.Option(STATEOPTION, "Configuration file(CSV)", CommandOptionType.MultipleValue);
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
                                foreach( var stateCode in stateCodes)
                                {
                                    if( stateCode.Trim().ToLower()== ALLSTATES)
                                    {
                                        allStates = true;
                                    }
                                }

                                if(allStates)
                                {
                                    // call for all states
                                    if(!crawlXmlAndSave.CrawlAllStates())
                                    {
                                        Console.WriteLine($"Failed to Crawl All States");
                                    }
                                }
                                else
                                {
                                    foreach (var stateCode in stateCodes)
                                    {
                                        if(!crawlXmlAndSave.CrawlSpecificState(stateCode))
                                        {
                                            Console.WriteLine($"Failed to Crawl {stateCode}");
                                        }
                                    }
                                }

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