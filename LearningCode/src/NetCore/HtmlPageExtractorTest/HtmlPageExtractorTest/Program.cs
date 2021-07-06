using System;
using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;
using HtmlAgilityPack;

namespace HtmlPageExtractorTest
{
    class Program
    {
        private const string CRAWL = "crawl";
        private const string HELP = "-? | -h | --help";
        static void Main(string[] args)
        {
            Console.WriteLine("HTML Extraction Test");
            var app = new CommandLineApplication();

            app.Command(CRAWL, config =>
            {
                var urls = config.Argument("<urls>","",true);
                config.OnExecute(() => { ExtractDataFromDoc(urls.Values); return 0; });
            });

            app.HelpOption(HELP);
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine(" {0} -h for help", System.AppDomain.CurrentDomain.FriendlyName);
                }
                else
                {
                    var result = app.Execute(args);
                    Console.WriteLine(" Result of App Execute .. {0}", result);
                }
            }
            catch (CommandParsingException )
            {
                Console.WriteLine("{0} -h for help", System.AppDomain.CurrentDomain.FriendlyName);
            }
        }

        static void ExtractDataFromDoc(IList<string> urls)
        {
            foreach( var url in urls)
            {
                Console.WriteLine($"Prasing URL :'{url}'");
                //ParseAndPrint(url);
                ParseZillowHomesAndPrintStates(url);
            }

        }

        static void ParseAndPrint( string url)
        {
            HtmlWeb web = new HtmlWeb();

            try
            {
                var htmlDoc = web.Load(url);
                // 
                var node = htmlDoc.DocumentNode.SelectSingleNode("//head/title");

                Console.WriteLine("Node Name: " + node.Name + "\n" + node.OuterHtml);
            }catch (Exception ex)
            {
                Console.WriteLine($"Exception:  '{ex.Message}'");
            }
        }

        static void ParseZillowHomesAndPrintStates(string url)
        {
            HtmlWeb web = new HtmlWeb();

            try
            {
                var htmlDoc = web.Load(url);
                

                // Let us test for  US states only for now.. 

                var countryNode = htmlDoc.DocumentNode.SelectSingleNode("//h2[.='United States']");

                PrintStates(url, countryNode);

                countryNode = htmlDoc.DocumentNode.SelectSingleNode("//h2[.='Canada']");
                PrintStates(url, countryNode);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception:  '{ex.Message}'");
            }
        }

        static void PrintStates( string url, HtmlNode countyNode)
        {
            try
            {

                if (countyNode != null)
                {
                    Console.WriteLine($" Country : {countyNode.InnerText} ");
                    //var stateNodes = htmlDoc.DocumentNode.SelectNodes("//ul[@class='bh-body-links']/li/a");
                    //var stateNodes = usStateH2Node.ParentNode.SelectNodes("./ul[@class='bh-body-links']/li/a");
                    var stateNodes = countyNode.SelectNodes("../ul[@class='bh-body-links']/li/a");

                    foreach (var node in stateNodes)
                    {
                        //Console.WriteLine(" SubNode Name: " + node.Name + "\n" + node.InnerHtml + " Outter"+ node.OuterHtml);
                        var stateUrl = GetAbsoluteUrlString(url, node.Attributes["href"].Value);
                        var stateName = node.InnerHtml;

                        Console.WriteLine($" State: '{stateName}', URL: '{stateUrl}'");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception:  '{ex.Message}'");
            }

        }

        static string GetAbsoluteUrlString(string baseUrl, string url)
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
                uri = new Uri(new Uri(baseUrl), uri);
            return uri.ToString();
        }
    }
}
