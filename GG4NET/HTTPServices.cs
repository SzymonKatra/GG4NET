using System.Text;
using System.Net;
using System.IO;

namespace GG4NET
{
    /// <summary>
    /// Usługi HTTP do serwera GG
    /// </summary>
    public static class HTTPServices
    {
        private static string StringRequest(string url, string method, bool readline = false, byte[] postData = null)
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
                if (method == "POST")
                {
                    BinaryWriter writer = new BinaryWriter(request.GetRequestStream());
                    writer.Write(postData);
                }
                response = (HttpWebResponse)request.GetResponse();
                StreamReader httpStream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("windows-1250"));
                responseStr = (readline) ? httpStream.ReadLine() : httpStream.ReadToEnd();
                response.Close();
            }
            catch { }
            return responseStr;
        }
        private static Stream DataRequest(string url, string method)
        {
            Stream responseStream = null;
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
                responseStream = response.GetResponseStream();
                //responseData = new BinaryReader(rStream).ReadBytes((int)rStream.Length);
            }
            catch { }
            return responseStream;
        }

        /// <summary>
        /// Znajdź serwer.
        /// </summary>
        /// <param name="num">Numer GG</param>
        /// <returns>IP znalezionego serwera</returns>
        public static IPAddress ObtainServer(uint num)
        {
            string url = string.Format("http://appmsg.gadu-gadu.pl/appsvc/appmsg4.asp?fmnumber={0}&version=8,0,0,7669&fmt=2", num);
            string response = StringRequest(url, "GET", true);
            if (response == null)
                return null;
            string[] responseParts = response.Split(' ');
            if (responseParts.Length >= 4)
                return IPAddress.Parse(responseParts[3]);
            return IPAddress.None;
        }

        /// <summary>
        /// Zdobądź token.
        /// </summary>
        /// <param name="tokenData">Dane tokenu</param>
        /// <param name="width">Szerokość tokenu</param>
        /// <param name="height">Wysokość tokenu</param>
        /// <param name="length">Długość tokenu</param>
        /// <param name="id">Indetyfikator tokenu</param>
        /// <returns>Bitmapa zawierająca token</returns>
        public static System.Drawing.Bitmap GetToken(out byte[] tokenData, out int width, out int height, out int length, out string id)
        {
            string firstResponse = StringRequest("http://register.gadu-gadu.pl/appsvc/regtoken.asp", "GET", false);
            string addr = string.Empty;
            using (StreamReader reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(firstResponse))))
            {
                string[] size = reader.ReadLine().Split(' ');
                width = int.Parse(size[0]);
                height = int.Parse(size[1]);
                length = int.Parse(size[2]);
                id = reader.ReadLine();
                addr = reader.ReadLine() + "?tokenid=" + id;
                reader.Close();
            }
            tokenData = null;
            return new System.Drawing.Bitmap(DataRequest(addr, "GET"));
        }
    }
}
