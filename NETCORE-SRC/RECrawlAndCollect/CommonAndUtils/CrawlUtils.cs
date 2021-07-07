using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.IO;

namespace CommonAndUtils
{
    public class CrawlUtils
    {
        public static HtmlDocument getHtmlDocFromUrl(String url)
        {
            try
            {

                HttpWebRequest httpReq = WebRequest.Create(url) as HttpWebRequest;

                httpReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                httpReq.Headers.Add(HttpRequestHeader.Connection, "keep-alive");
                //httpReq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 Edg/91.0.864.64";
                httpReq.UserAgent = "Mediapartners-Google";
                //httpReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
                httpReq.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9,te;q=0.8");


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
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($" Exception While Cralwing: {url}, Exception Message: {ex.Message}");
            }
            return null;
        }
    }
}
