using System;

namespace CrawlerLib
{
    public class Crawler
    {
        private const string ZILLOW = "www.zillow.com";
        public void CrawlUrl(string url)
        {
            var uri = new Uri(url);

            switch (uri.Host)
            {
                case ZILLOW:
                    var crawler = new ZillowCrawler();
                    crawler.CrawlProperties(uri);
                    break;
                default:
                    break;
            }
        }

        public bool IsReady()
        {
            return true;
        }
    }
}
