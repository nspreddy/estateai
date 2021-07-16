using System;
using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;
using DataModels;
using CommonAndUtils;
using CrawlerLib;

namespace PropAnalyzer
{
    class PropAnalyzer
    {
        private const string GEOCFGFILEOPTION = "-g|--geodb <value>";
        private const string PROCESSCONFIG    = "-p|--process <value>";
        private const string INPUTDIROPTION   = "-i|--inputdir <value>";
        private const string OUTPUTDIROPTION  = "-o|--outputdir <value>";
        private const string DATEOPTION       = "-d|--date <value>";

        private const string HELP             = "-? | -h | --help";
        private const string VERSIONOPTION    = "-v|--version";
        private const string VERSION          = "0.1";

        private const string DESC = "Property Data Analysis";
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

            #region OPTIONS
            var geoconfigFileOption = app.Option(GEOCFGFILEOPTION, "GeoDB Configfile", CommandOptionType.SingleValue);
            var outputDirOption = app.Option(OUTPUTDIROPTION, "OutPut DIR", CommandOptionType.SingleValue);
            var inputDirOption = app.Option(INPUTDIROPTION, "Input DIR", CommandOptionType.SingleValue);
            var processConfigOption = app.Option(PROCESSCONFIG, "Crawl Config File", CommandOptionType.MultipleValue);
            var dateOption = app.Option(DATEOPTION, "Date option", CommandOptionType.SingleValue);
            #endregion

            app.OnExecute(() =>
            {
                var geoConfigFile = geoconfigFileOption.Value();
                var dirToSave = outputDirOption.Value();
                var dirToReadXmlFiles = inputDirOption.Value();
                var processConfigFiles = processConfigOption.Values;
                var dateInput =  dateOption.Value();               
                var returnValue = AnalyzeData(geoConfigFile, dirToReadXmlFiles, processConfigFiles,dateInput, dirToSave);
                if (returnValue == -1)
                {
                    Console.WriteLine($" HELP: {execName} {HELP}");
                }
                return returnValue;
            });

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

        private static int AnalyzeData(string geoConfigFile, string dirToReadHtmlFiles, List<string> processConfigFiles, string dateInput, string dirToSave)
        {
            int returnValue = -1;

            if (!string.IsNullOrEmpty(geoConfigFile) &&
                !string.IsNullOrEmpty(dirToReadHtmlFiles) &&
                processConfigFiles != null &&
                processConfigFiles.Count > 0)
            {
                if (string.IsNullOrEmpty(dirToSave))
                {
                    dirToSave = ".";
                }

                if (string.IsNullOrEmpty(dateInput))
                {
                    dateInput = DateTime.Now.ToString("yyyy_MM_dd");
                }


                Console.WriteLine($"Parsing file {geoConfigFile} for Geo Data nad output will be written to Directory {dirToSave}");
                if (GeoData.LoadGeoDBFromJsonFile(geoConfigFile))
                {
                    if (processConfigFiles != null && processConfigFiles.Count > 0)
                    {
                        foreach (var processConfig in processConfigFiles)
                        {
                            ProcessConfigAndStartAnalyzing(processConfig, dirToReadHtmlFiles, dateInput, dirToSave);
                        }
                        // Wait for Crawler to finish procssing all the Jobs.
                        // Queued all the work, let us kick off Threads to do their job. 
                        CrawlerFramework.KickoffJobAgents();
                        // Wait for all the work to be finished by Threads.
                        CrawlerFramework.WaitForAllJobstoComplete();
                        Console.WriteLine(" All Jobs in queue processed, hence exiting");
                        returnValue = 0;
                    }
                    else
                    {
                        Console.WriteLine($"Process Configuration File missing");
                    }
                }
                else
                {
                    Console.WriteLine($"Unable to read geoDB  file: {geoConfigFile}");
                }
            }
            else
            {
                Console.WriteLine($" Error in input params geoDB:{geoConfigFile}, XMlInputFilesDir:{dirToReadHtmlFiles}");
            }
            return returnValue;
        }


        private static bool ProcessConfigAndStartAnalyzing(string configFile, string inputDir, string date, string outputDir)
        {
            bool returnValue = false;

            try
            {
                // Let us get JSON object from configuarion file. 
                var processConfig = CrawlerInputConfig.GetCrawlerConfigurationFromJsonFile(configFile);
                if (processConfig != null)
                {
                    // let us get the XML File based on State. 
                    var stateObject = GeoData.DefaultNation.GetState(processConfig.State);
                    if (stateObject != null)
                    {
                        var analysisHelper = new AnalysisHelper(processConfig,inputDir,date, outputDir);
                        analysisHelper.PrepListOfProperyHtmlPages2Extract();
                        analysisHelper.PrepListOfStatsHtmlPages2Extract();
                    }
                    else
                    {
                        Console.WriteLine($"State:{processConfig.State} not found in the given GeoData");
                    }
                }
                else
                {
                    Console.WriteLine($"Unable to read Config(JSON): {configFile}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to  process config: {configFile}, Exception: {ex.Message}");
            }
            return returnValue;
        }

    }
}
