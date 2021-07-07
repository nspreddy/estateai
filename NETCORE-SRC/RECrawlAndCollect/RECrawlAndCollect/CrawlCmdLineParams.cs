﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlerLib;

namespace RECrawlAndCollect
{
    class CrawlCmdLineParams
    {        
        static public bool ProcessUrls( List<string> urls)
        {
            bool returnValue = false;
            if (urls == null || urls.Count == 0)
            {
                Console.WriteLine(" Urls is null of zero count, cannot crawl");
               
            }
            else
            {
                Crawler crawlerObject = new Crawler();
                if (crawlerObject.IsReady())
                {
                    foreach (var url in urls)
                    {
                        crawlerObject.CrawlUrl(url);
                    }
                    returnValue = true;
                }               
            }
            return returnValue;
        }

        static public bool ProcessConfigurationFile( string crawlDefinitionFileName)
        {
            return true;
        }
    }
}