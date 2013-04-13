using System;
using System.Text;
using System.Net;
using System.IO;

namespace GG4NET
{
    public static class HTTPServices
    {
        private static string Request(string url, string method, bool readline)
        {
            string responseStr = string.Empty;
            HttpWebRequest request;
            HttpWebResponse response;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 5.0; Windows NT; DigExt)";
                request.ContentLength = 0;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = method;
                response = (HttpWebResponse)request.GetResponse();
                StreamReader httpStream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("windows-1250"));
                responseStr = (readline) ? httpStream.ReadLine() : httpStream.ReadToEnd();
                response.Close();
            }
            catch { }
            return responseStr;
        }

        public static IPAddress ObtainServer(uint num)
        {
            string url = string.Format("http://appmsg.gadu-gadu.pl/appsvc/appmsg4.asp?fmnumber={0}&version=8,0,0,7669&fmt=2", num);
            string response = Request(url, "GET", true);
            if (response == null)
                return null;
            string[] responseParts = response.Split(' ');
            if (responseParts.Length >= 4)
                return IPAddress.Parse(responseParts[3]);
            return IPAddress.None;
        }
    }
}
