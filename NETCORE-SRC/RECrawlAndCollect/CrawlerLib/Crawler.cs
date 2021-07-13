using System;
using CommonAndUtils;

namespace CrawlerLib
{
    internal  class Crawler
    {
        private const string ZILLOW = "www.zillow.com";
        private const string REDFIN = "www.redfin.com";
        internal  bool  CrawlUrl(string url)
        {
            bool returnValue = false; ;
            var uri = new Uri(url);

            switch (uri.Host)
            {
                case ZILLOW:
                    var crawler = new ZillowCrawler();
                    crawler.CrawlProperties(uri);
                    returnValue = true;
                    break;
                case REDFIN:
                    RedfinCrawler redfinCrawler = new RedfinCrawler();
                    redfinCrawler.CrawlProperties(uri);
                    returnValue = true;
                    break;
                default:
                    break;
            }
            return returnValue;
        }



        internal bool CrawlRedfinXMLContent(string url, string filepath)
        {
            return CrawlAndSavePayload(url, filepath);
        }

        private bool CrawlAndSavePayload( string url, string filepath)
        {
            bool returnValue = false;

            try
            {
                var crawledDoc = CrawlUtils.getHtmlDocFromUrl(url);
                if( crawledDoc != null)
                {                   
                    crawledDoc.Save(filepath);
                    returnValue = true;
                }

            }catch( Exception ex)
            {
                Console.WriteLine($" Unbale to either Crawl {url} or save File @ {filepath}, exception: {ex.Message}");
            }

            return returnValue;
        }
       
    }
}
