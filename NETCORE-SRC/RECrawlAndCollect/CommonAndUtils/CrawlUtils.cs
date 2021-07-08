using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace CommonAndUtils
{
    public class CrawlUtils
    {
        private const string BINGBOT = "bingbot";
        private const string GOOLEADSBOT = "AdsBot-Google";
        private const string DEFAULT_UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 Edg/91.0.864.64";
        private const string ACCEPT_STRING = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
        private const string KEEP_ALIVE  = "keep-alive";
        private const string ACCEPT_LANG = "en-US,en;q=0.9,te;q=0.8";
        private const string ENCODING_OPTION = "gzip, deflate, br";


        public static void AddAcceptHeaders(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ACCEPT_STRING));            

        }

        public static HtmlDocument getHtmlDocFromUrlAsync(String url)
        {
            try
            {

                //HttpWebRequest httpReq = WebRequest.Create(url) as HttpWebRequest;
                HttpClient httpClient = new HttpClient();

                //httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.TryParseAdd(ACCEPT_STRING);
                httpClient.DefaultRequestHeaders.AcceptCharset.TryParseAdd(ACCEPT_LANG);
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(BINGBOT);
                httpClient.DefaultRequestHeaders.Connection.TryParseAdd(KEEP_ALIVE);
               
                /*
                httpReq.Accept = ACCEPT_STRING;                
                httpReq.Headers.Add(HttpRequestHeader.Connection, KEEP_ALIVE);
                //httpReq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 Edg/91.0.864.64";
                httpReq.UserAgent = BINGBOT;
                //httpReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
                httpReq.Headers.Add(HttpRequestHeader.AcceptLanguage, ACCEPT_LANG);*/

                var result =  httpClient.GetAsync(url).Result;           

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($" Statud Code of http Request..{ result.StatusCode}");
                }
                else
                {
                    string htmlText = result.Content.ReadAsStringAsync().Result;
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(htmlText);
                    return htmlDoc;
                }
                /*
                HttpWebResponse response = (HttpWebResponse)httpReq.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader htmlRspStream = new StreamReader(response.GetResponseStream());
                    string htmlText = htmlRspStream.ReadToEnd();
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(htmlText);
                    return htmlDoc;
                }
                else
                {
                    Console.WriteLine($" Statud Code of http Request..{ response.StatusCode}");
                }*/

            }
            catch (Exception ex)
            {
                Console.WriteLine($" Exception While Cralwing: {url}, Exception Message: {ex.Message}");
            }
            return null;
        }
    }
}
