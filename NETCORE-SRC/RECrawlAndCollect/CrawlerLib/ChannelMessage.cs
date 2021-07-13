using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerLib
{
    internal class ChannelMessage
    {
         public enum ActionType { CRAWL_REDFIN_XMLDATA, CRAWL_PROP_EXTRACT, CRAWL_STATS, CRAWL_SAVE, PROCESS_RECORD};
        public long ID { get; set; }
        public  ActionType Action { get; set;}

        public string Url { get; set;}
        public object Payload;
    }
}
