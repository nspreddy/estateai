using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerLib
{
    internal class ChannelMessage
    {
         public enum ActionType { CRAWL_URL_SAVE2FILE,DU_PROPS, DU_STATS, CRAWL_URLS, PROCESS_RECORD};
        public Guid ID { get; set; }
        public  ActionType Action { get; set;}

        public string Url { get; set;}
        public object Payload;
    }
}
