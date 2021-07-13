using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Threading;


namespace CrawlerLib
{
    public class CrawlerFramework
    {
        private const int MIN_THREAD_COUNT = 5;
        private const int INITIAL_COUNT_DOWN_COUNT = 1;
        private static CountdownEvent cde = new CountdownEvent(INITIAL_COUNT_DOWN_COUNT);
        public static bool KickoffJobAgents(int minThreadCount= MIN_THREAD_COUNT)
        {
            bool returnValue = false;
            try
            {
                Console.WriteLine($"Kikcing off Message processors {minThreadCount}");
                for(int i=0;i< minThreadCount; i++)
                {
                    ThreadPool.QueueUserWorkItem(CrawlerJobAgent, ChannelManager.Crawler_Queue);
                    //ThreadPool.QueueUserWorkItem(CrawlerJobAgent, ChannelManager.Analyzer_Queue);
                }
            }catch (Exception ex)
            {
                Console.WriteLine($"Unable to kick off Frame work {ex.Message}");
            }
            cde.Signal();// Signal that Thread creation JOb is done. 
            return returnValue;
        }

        public static void WaitForAllJobstoComplete()
        {
            try
            {
                cde.Wait();
            }catch(Exception ex)
            {
                Console.WriteLine($"Exception while waiting for other jobs to finish {ex.Message}");
            }
        }

        public static bool QueueCrawlRedfinXMLContentJob(string url, string filepath)
        {
            ChannelMessage message = new ChannelMessage();

            message.Action = ChannelMessage.ActionType.CRAWL_REDFIN_XMLDATA;
            message.Url = url;
            message.Payload = filepath;
            cde.AddCount(1);
            var writeTask = ChannelManager.WriteMessageFromChannelAsync(ChannelManager.Crawler_Queue, message);
            if (writeTask == null)
            {
                cde.Signal();// since we failed to queue, hence decrement this
                return false;
            }
            else
            {
                return writeTask.Result;
            }
        }

        public static bool QueueCrawlURLsJob(string url)
        {
            ChannelMessage message = new ChannelMessage();

            message.Action = ChannelMessage.ActionType.CRAWL_SAVE;
            message.Url = url;
            message.Payload = null;
            cde.AddCount(1);
            var writeTask = ChannelManager.WriteMessageFromChannelAsync(ChannelManager.Crawler_Queue, message);
            if (writeTask == null)
            {
                cde.Signal();// since we failed to queue, hence decrement this
                return false;
            }
            else
            {
                return writeTask.Result;
            }
        }


        #region PRIVATE_METHODS
        private static void CrawlerJobAgent( object channelNameToListen)
        {
            string channelName = channelNameToListen as string;
           
            if(!string.IsNullOrEmpty(channelName))
            {
                bool exitFlag = false;
                Console.WriteLine($"Starting to Listen {channelName} , Thr ID: {Thread.CurrentThread.ManagedThreadId}");
                while (!exitFlag)
                {
                    var readTask = ChannelManager.ReadMessageFromChannelAsync(channelName);

                    if (readTask != null)
                    {
                        var message = readTask.Result;
                        if (message != null)
                        {
                            var returnValue= ProcessMessage(message);
                            Console.WriteLine($"Job Status for Job ID: {message.ID}, Status: {returnValue}");
                            cde.Signal();
                        }
                        else
                        {
                            Console.WriteLine($"Null Message, Channel name: {channelName}");
                        }
                        
                    }
                    else
                    {
                        Console.WriteLine($"Unable to read from Channel {channelName}, Exiting");
                        exitFlag = true;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Empty Channel name, Exiting Thread { Thread.CurrentThread.ManagedThreadId}");
            }
        }

        private static bool ProcessMessage( ChannelMessage message)
        {
            bool returnValue = false;
            Console.WriteLine($"Message recieved, details: {message.Action}, URL: {message.Url}");
            switch (message.Action)
            {
                case ChannelMessage.ActionType.CRAWL_PROP_EXTRACT:
                    break;
                case ChannelMessage.ActionType.CRAWL_REDFIN_XMLDATA:
                    returnValue = CrawlRedfinXMLData(message);
                    break;
                case ChannelMessage.ActionType.CRAWL_STATS:
                    break;
                case ChannelMessage.ActionType.CRAWL_SAVE:
                    returnValue=CrawlURLsAndSaveContent(message);
                    break;
                case ChannelMessage.ActionType.PROCESS_RECORD:
                    break;
            }
            Console.WriteLine($"Job Status: Msg ID: { message.ID}, {returnValue}");
            return returnValue;
        }

        private static bool CrawlRedfinXMLData(ChannelMessage message)
        {
            var returnValue = false;
            var redfinUrl = message.Url;
            var filepath = message.Payload as string;
            if( !string.IsNullOrEmpty(redfinUrl)  && !string.IsNullOrEmpty(filepath))
            {
                var crawler = new Crawler();
                returnValue = crawler.CrawlRedfinXMLContent(redfinUrl, filepath);
            }
            else
            {
                Console.WriteLine($" Message key params missing. url:{redfinUrl} , payload : { filepath}");
            }
            return returnValue;
        }

        private static bool CrawlURLsAndSaveContent(ChannelMessage message)
        {
            var returnValue = false;
            var url = message.Url;
            if (!string.IsNullOrEmpty(url) )
            {
                var crawler = new Crawler();
                returnValue = crawler.CrawlUrl(url);
            }
            else
            {
                Console.WriteLine($" Message key params missing. url:{url} ");
            }
            return returnValue;
        }

        #endregion


    }
}
