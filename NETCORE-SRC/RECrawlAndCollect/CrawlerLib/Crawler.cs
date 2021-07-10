using System;
using CommonAndUtils;

namespace CrawlerLib
{
    public class Crawler
    {
        private const string ZILLOW = "www.zillow.com";
        private const string REDFIN = "www.redfin.com";
        public void CrawlUrl(string url)
        {
            var uri = new Uri(url);

            switch (uri.Host)
            {
                case ZILLOW:
                    var crawler = new ZillowCrawler();
                    crawler.CrawlProperties(uri);
                    break;
                case REDFIN:
                    RedfinCrawler redfinCrawler = new RedfinCrawler();
                    redfinCrawler.CrawlProperties(uri);
                    break;
                default:
                    break;
            }
        }

        public bool CrawlAndSavePayload( string url, string filepath)
        {
            bool returnValue = false;

            try
            {
                var crawledDoc = CrawlUtils.getHtmlDocFromUrl(url);
                if( crawledDoc != null)
                {                   
                    crawledDoc.Save(filepath);
                }

            }catch( Exception ex)
            {
                Console.WriteLine($" Unbale to either Crawl {url} or save File @ {filepath}, exception: {ex.Message}");
            }

            return returnValue;
        }

        public bool IsReady()
        {
            return true;
        }
    }
}
