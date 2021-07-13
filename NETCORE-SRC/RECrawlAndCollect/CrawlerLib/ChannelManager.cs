using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;


namespace CrawlerLib
{
    public class ChannelManager
    {
        public const string Crawler_Queue  = "CrawlerQueue";
        public const string Analyzer_Queue = "AnalyzerQueue";

        private static Dictionary<string, Channel<ChannelMessage>> ChannelDict = new Dictionary<string, Channel<ChannelMessage>>();

        private static Channel<ChannelMessage> GetChannel( string channelname)
        {
            Channel<ChannelMessage> channelObject = null;

            try
            {
                lock (ChannelDict)
                {
                    if (!ChannelDict.TryGetValue(channelname, out channelObject))
                    {
                        channelObject = Channel.CreateUnbounded<ChannelMessage>();
                        ChannelDict.Add(channelname, channelObject);
                    }
                }
            }
            catch(Exception ex)
            {
                channelObject = null;
                Console.WriteLine($"Exception while getting channel for {channelname}, Exception: {ex.Message}");
            }

            return channelObject;
        }

        public static async Task<bool> WriteMessageFromChannelAsync(string channelname, ChannelMessage msg)
        {
            Channel<ChannelMessage> channelObject = null;
            bool returnValue = false;
            try
            {
                channelObject = GetChannel(channelname);
                if( channelObject != null)
                {
                    await channelObject.Writer.WriteAsync(msg);
                    returnValue = true;
                }                
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Exception while Writing Message to  channel for {channelname}, Exception: {ex.Message}");
            }

            return returnValue;
        }

        public static async Task<ChannelMessage> ReadMessageFromChannelAsync(string channelname)
        {
            Channel<ChannelMessage> channelObject = null;
            ChannelMessage msg = null;

            try
            {
                channelObject = GetChannel(channelname);
                if (channelObject != null)
                {
                    msg = await channelObject.Reader.ReadAsync();
                }
            }
            catch (Exception ex)
            {                
                Console.WriteLine($"Exception while Reading Message to  channel for {channelname}, Exception: {ex.Message}");
            }

            return msg;
        }


    }
}
