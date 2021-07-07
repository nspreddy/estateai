using System;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace RECrawlAndCollect
{
    class Program
    {
        private const string DESC = "Real estate data crawler";
        private const string CRAWL = "crawl";
        private const string HELP = "-? | -h | --help";
        private const string VERSIONOPTION = "-v|--version";
        private const string VERSION = "0.1";

        // Crawl Options 
        private const string URLOPTION     = "-u|--urls <value>";
        private const string CFGFILEOPTION = "-f|--file <value>";
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

            #region CRAWL_ACTIONS
            app.Command(CRAWL, config =>
            {                
                var configFileOption = config.Option(CFGFILEOPTION,"Configuration file(Yaml)",CommandOptionType.SingleValue);
                var urlsOption = config.Option(URLOPTION, "Multiple URLs to crawl", CommandOptionType.MultipleValue);
                config.HelpOption(HELP);
                
                config.OnExecute(() =>
                {
                    bool optionExecuted = false;

                    if (urlsOption.HasValue())
                    {
                        // Let us crawl thoses URLs.
                        var urls = urlsOption.Values;
                        CrawlCmdLineParams.ProcessUrls(urls);
                        optionExecuted = true;
                    }

                    if (configFileOption.HasValue())
                    {
                        var configFile = configFileOption.Value();
                        Console.WriteLine($"Parsing file {configFile} to crawl");
                        optionExecuted = true;
                    }


                    if (!optionExecuted)
                    {
                        Console.WriteLine($" OPT-HELp: {execName} {CRAWL} {HELP}");
                    }

                    return 0;
                });
            });
            #endregion

            #region NO_ARGS

            app.OnExecute(() => 
               {
                   Console.WriteLine($" HELP: {execName} {HELP}");                    
                   return 0;
               }
             );
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
                    Console.WriteLine($"Result of {execName} Execute .. {result}");
                }
            }
            catch (CommandParsingException)
            {
                Console.WriteLine($" HELP: {execName} {HELP}");
            }                     

        }
    }
}
