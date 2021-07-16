using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Threading;


namespace CrawlerLib
{
    public partial class CrawlerFramework
    {
        #region PRIVATE_MEMEBERS
        private const int MIN_THREAD_COUNT = 5;
        private const int INITIAL_COUNT_DOWN_COUNT = 1;
        private const int LATENCY_MAX_BETWEEK_JOBS = 500;// in milliseconds
        private static CountdownEvent cde = new CountdownEvent(INITIAL_COUNT_DOWN_COUNT);

        private static int JobsQueued = 0;
        private static int JobsCompletedWithSuccess = 0;
        private static int JobsCompletedWithFailure = 0;
        private static bool LatencyFlag = true;
        #endregion

        #region PUBLIC_MEMBERS
        public static int JOBS_IN_QUEUE { get { return JobsQueued; } }
        public static int JOBS_PASSED { get { return JobsCompletedWithSuccess; } }
        public static int JOBS_FAILED { get { return JobsCompletedWithFailure; } }

        #endregion

        public static bool KickoffJobAgents(bool latencyFalg=true,int minThreadCount= MIN_THREAD_COUNT)
        {
            bool returnValue = false;
            LatencyFlag= latencyFalg;
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

        public static bool QueueCrawlUrAndSave2FileJob(string url, string filepath)
        {
            return SubmitCrawlJob(url, ChannelMessage.ActionType.CRAWL_URL_SAVE2FILE, filepath);            
        }

        public static bool QueueExtractProperyDataFromHTML(string fileUrl, string outputFile)
        {
            return SubmitCrawlJob(fileUrl, ChannelMessage.ActionType.DU_PROPS, outputFile);
        }

        public static bool QueueExtractStatsFromHTML(string fileUrl, string outputFile)
        {
            return SubmitCrawlJob(fileUrl, ChannelMessage.ActionType.DU_STATS, outputFile);
        }

        public static bool QueueCrawlURLsJob(string url)
        {
            return SubmitCrawlJob(url, ChannelMessage.ActionType.CRAWL_URLS, null);            
        }

        #region PRIVATE_METHODS

        

        private static bool SubmitCrawlJob(string url,ChannelMessage.ActionType action,object payload)
        { 
            bool returnValue = false;
            try
            {
                cde.AddCount(1);
                ChannelMessage message = new ChannelMessage();
                message.Action = action;
                message.Url = url;
                message.Payload = payload;
                message.ID = Guid.NewGuid();
                var writeTask = ChannelManager.WriteMessageFromChannelAsync(ChannelManager.Crawler_Queue, message);
                if (writeTask == null)
                {
                    cde.Signal();// since we failed to queue, hence decrement this
                }
                else
                {
                    IncrementJobQueued();
                    returnValue= writeTask.Result;
                }
            }catch(Exception ex)
            {
                cde.Signal();
                Console.WriteLine($"Exception while Queueing Message to process, {ex.Message}");
            }
            return returnValue;
        }


        private static void IncrementJobQueued()
        {
            Interlocked.Increment(ref JobsQueued);
        }

        private static void IncrementJobsCompletedWithFailure()
        {
            Interlocked.Increment(ref JobsCompletedWithFailure);
        }

        private static void IncrementJobsCompletedWithSucess()
        {
            Interlocked.Increment(ref JobsCompletedWithSuccess);            
        }
        #endregion


    }
}
