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
using System.IO.Compression;

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
        private const string ENCODING_OPTION = "gzip, deflate";
        private const string CONTENT_TYPE_HTML = "text/html";
        private const string CONTENT_TYPE_XML = "text/xml";
        private const string CONTENT_TYPE_GZIP = "application/x-gzip";


        public static void FillHeaders(HttpClient httpClient)
        {
            //httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.TryParseAdd(ACCEPT_STRING);
            httpClient.DefaultRequestHeaders.AcceptCharset.TryParseAdd(ACCEPT_LANG);
            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(BINGBOT);
            httpClient.DefaultRequestHeaders.Connection.TryParseAdd(KEEP_ALIVE);
            httpClient.DefaultRequestHeaders.AcceptEncoding.TryParseAdd(ENCODING_OPTION);
        }

        public static HtmlDocument getHtmlDocFromUrl(String url)
        {
            HtmlDocument Doc = null;
            try
            {
                // automatically handle gzip decomression.
                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }

                HttpClient httpClient = new HttpClient(handler);
                FillHeaders(httpClient);
                var result =  httpClient.GetAsync(url).Result;           

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($" Statud Code of http Request..{ result.StatusCode}");
                }
                else
                {
                    // check the output is HTML or XML.
                    var contentType = result.Content.Headers.ContentType.MediaType;

                    switch (contentType)
                    {
                        case CONTENT_TYPE_HTML:
                        case CONTENT_TYPE_XML:
                            string htmlText = result.Content.ReadAsStringAsync().Result;
                            if(!string.IsNullOrEmpty(htmlText))
                            {
                                Doc = new HtmlDocument();
                                Doc.LoadHtml(htmlText);
                            }                            
                            break;
                        case CONTENT_TYPE_GZIP:
                            using (var responseStream = result.Content.ReadAsStreamAsync().Result)
                            {
                                if (responseStream != null)
                                {
                                    using (var gzipStream = new GZipStream(responseStream, CompressionMode.Decompress))
                                    {
                                        if (gzipStream != null)
                                        {
                                            using (var sr = new StreamReader(gzipStream))
                                            {
                                                if (sr != null)
                                                {
                                                    var xmlText = sr.ReadToEnd();
                                                    Doc = new HtmlDocument();
                                                    Doc.LoadHtml(xmlText);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    
                }               

            }
            catch (Exception ex)
            {
                Console.WriteLine($" Exception While Cralwing: {url}, Exception Message: {ex.Message}");
            }
            return Doc;
        }

        public static HtmlDocument GethtmlDocFromXmlOrHtmlFile( string filepath)
        {
            HtmlDocument Doc = null;

            try
            {
                string xmlText = File.ReadAllText(filepath);
                if(!string.IsNullOrEmpty(xmlText))
                {
                    Doc = new HtmlDocument();
                    Doc.LoadHtml(xmlText);
                }

            }catch(Exception ex)
            {
                Console.WriteLine($"Failed to read file:{filepath}, exception:{ex.Message}");
            }
            return Doc;
        }
    }
}
