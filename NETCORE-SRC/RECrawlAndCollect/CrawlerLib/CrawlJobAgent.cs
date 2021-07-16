using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RedfinUtils;


namespace CrawlerLib
{
    public partial class CrawlerFramework
    {
        private  static void CrawlerJobAgent(object channelNameToListen)
        {
            string channelName = channelNameToListen as string;

            if (!string.IsNullOrEmpty(channelName))
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
                            var returnValue = ProcessMessage(message);
                            if (returnValue)
                            {
                                IncrementJobsCompletedWithSucess();
                            }
                            else
                            {
                                IncrementJobsCompletedWithFailure();
                            }

                            Console.WriteLine($"Job Status for Job ID: {message.ID}, Status: {returnValue}");
                            cde.Signal();
                            if (LatencyFlag)
                            {
                                Thread.Sleep(LATENCY_MAX_BETWEEK_JOBS);
                            }
                                
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

        private static bool ProcessMessage(ChannelMessage message)
        {
            bool returnValue = false;
            Console.WriteLine($"Message recieved, details: {message.Action}, URL: {message.Url}");
            switch (message.Action)
            {
                case ChannelMessage.ActionType.DU_PROPS:
                    PerfromDUForProps(message);
                    break;
                case ChannelMessage.ActionType.DU_STATS:
                    PerfromDUForStats(message);
                    break;
                case ChannelMessage.ActionType.CRAWL_URL_SAVE2FILE:
                    returnValue = CrawlURLsAndSave2File(message);
                    break;
                case ChannelMessage.ActionType.PROCESS_RECORD:
                    break;
                case ChannelMessage.ActionType.CRAWL_URLS:
                    CrawlURLsAndSaveContent(message);
                    break;
            }
            Console.WriteLine($"Job Status: Msg ID: { message.ID}, {returnValue}");
            return returnValue;
        }


        private static bool CrawlURLsAndSave2File(ChannelMessage message)
        {
            var returnValue = false;
            var redfinUrl = message.Url;
            var filepath = message.Payload as string;
            if (!string.IsNullOrEmpty(redfinUrl) && !string.IsNullOrEmpty(filepath))
            {
                var crawler = new Crawler();
                returnValue = crawler.CrawlUrlAndSavePayload(redfinUrl, filepath);
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
            if (!string.IsNullOrEmpty(url))
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

        private static bool PerfromDUForProps(ChannelMessage message)
        {
            var returnValue = false;
            var htmlFile = message.Url;
            var outputCsv = (string)message.Payload;

            if (!string.IsNullOrEmpty(htmlFile) && !string.IsNullOrEmpty(outputCsv))
            {
                var redfinProphtmlDU = new RedfinPropHtmlDU(htmlFile,outputCsv);
                returnValue= redfinProphtmlDU.PerformDU();
                if(!returnValue)
                {
                    Console.WriteLine($"Failed to perform Props DU {htmlFile}");
                }
            }
            else
            {
                Console.WriteLine($"PROP DU: Message key params missing. url:{htmlFile} or {outputCsv}");
            }
            return returnValue;
        }

        private static bool PerfromDUForStats(ChannelMessage message)
        {
            var returnValue = false;
            var htmlFile = message.Url;
            var outputCsv = (string)message.Payload;

            if (!string.IsNullOrEmpty(htmlFile) && !string.IsNullOrEmpty(outputCsv))
            {
                var redfinStatshtmlDU = new RedfinStatsHtmlDU(htmlFile, outputCsv);
                returnValue = redfinStatshtmlDU.PerformDU();
                if (!returnValue)
                {
                    Console.WriteLine($"Failed to perform Stats DU {htmlFile}");
                }
            }
            else
            {
                Console.WriteLine($"STATS DU: Message key params missing. url:{htmlFile} or {outputCsv}");
            }
            return returnValue;
        }
    }
}
