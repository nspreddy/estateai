using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerLib
{
    class CrawlerBase
    {
        #region PRIVATE_MEMBERS
        private ConcurrentQueue<Uri> urlQueue = new ConcurrentQueue<Uri>();
        private ConcurrentDictionary<string, bool> crawledUrlsDictionary = new ConcurrentDictionary<string, bool>();

        #endregion

        #region PROTECTED_METHODS

        protected void AddUrlToTheQueue(string url)
        {
            if (!crawledUrlsDictionary.ContainsKey(url))
            {
                // Let us add to the queue
                urlQueue.Enqueue(new Uri(url));
            }
            else
            {
                Console.WriteLine($"Why we are adding already Crawled URL {url}");
            }
        }

        protected void MarkAsCrawled(string url)
        {
            if (!crawledUrlsDictionary.ContainsKey(url))
            {
                crawledUrlsDictionary[url] = true;
            }
            else
            {
                Console.WriteLine($"Why we are making as crawled for  already Crawled URL {url}");
            }
        }

        protected virtual bool ProcessURI(Uri uri)
        {
            return true;
        }
        #endregion

        public void CrawlProperties(Uri inputUri)
        {
            // determine URL scope .. 
            // https://www.zillow.com/browse/homes:  Each State date
            // https://www.zillow.com/browse/homes/wa/  : State specific .. list contain County properties and Sale Properties.
            // https://www.zillow.com/browse/homes/wa/king-county/newest-homes/ : List of Sale homes
            // https://www.zillow.com/homedetails/0-11875-175th-Pl-NE-Redmond-WA-98052/2141039322_zpid/ : Property

            urlQueue.Enqueue(inputUri);

            while (urlQueue.Count > 0)
            {
                // Basic Checks                
                Uri uri2Crawl;
                if (!urlQueue.TryDequeue(out uri2Crawl))
                {
                    Console.WriteLine(" Null Item from Queue.. Really Strange");
                    continue;
                }
                if (!ProcessURI(uri2Crawl))
                {
                    Console.WriteLine($"Failure to Crawl {uri2Crawl}");
                }

            }
        }
    }
}
