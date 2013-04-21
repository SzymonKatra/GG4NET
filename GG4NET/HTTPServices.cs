using System.Text;
using System.Net;
using System.IO;
using System;

namespace GG4NET
{
    /// <summary>
    /// Kod błędu dla usług GG HTTP.
    /// </summary>
    public enum GGHTTPError
    {
        /// <summary>Brak.</summary>
        None,
        /// <summary>Sukces.</summary>
        Success,
        /// <summary>Zła wartość tokenu.</summary>
        BadTokenValue,
        /// <summary>Hasło za długie.</summary>
        PasswordTooLong,
        /// <summary>Zły adres email.</summary>
        BadEmail,
        /// <summary>Złe stare hasło.</summary>
        BadOldPassword,
        /// <summary>Przypominanie hasła nie powiodło się.</summary>
        PasswordRemindFailed,
    }

    /// <summary>
    /// Usługi HTTP do serwera GG
    /// </summary>
    public static class HTTPServices
    {
        private static string StringRequest(string url, string method, bool readline = false, string postData = null)
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
                    if (postData != null && postData != string.Empty)
                    {
                        BinaryWriter writer = new BinaryWriter(request.GetRequestStream());
                        writer.Write(Encoding.UTF8.GetBytes(postData));
                    }
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
        /// Oblicza hash potrzebny do usług HTTP. Skopiowano z DotGadu, które skopiowało z Sharp THGG.
        /// </summary>
        /// <param name="hashParams">Parametry</param>
        /// <returns>Hash</returns>
        private static int Hash(string[] hashParams)
        {
            int b = -1, i, j;
            uint a, c;

            if (hashParams.Length == 0)
                return -1;

            for (i = 0; i < hashParams.Length; i++)
            {
                for (j = 0; j < hashParams[i].Length; j++)
                {
                    c = hashParams[i][j];
                    a = (uint)(c ^ b) + (c << 8);
                    b = (int)(a >> 24) | (int)(a << 8);
                }
            }
            return System.Math.Abs(b);
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

        #region Token
        /// <summary>
        /// Zdobądź token.
        /// </summary>
        /// <param name="width">Szerokość tokenu</param>
        /// <param name="height">Wysokość tokenu</param>
        /// <param name="length">Długość tokenu</param>
        /// <param name="id">Indetyfikator tokenu</param>
        /// <returns>Bitmapa zawierająca token</returns>
        public static System.Drawing.Image GetToken(out int width, out int height, out int length, out string id)
        {
            byte[] data = null;
            return GetToken(out width, out height, out length, out id, out data);
        }
        /// <summary>
        /// Zdobądź token.
        /// </summary>
        /// <param name="width">Szerokość tokenu</param>
        /// <param name="height">Wysokość tokenu</param>
        /// <param name="length">Długość tokenu</param>
        /// <param name="id">Indetyfikator tokenu</param>
        /// <param name="tokenData">Dane tokenu</param>
        /// <returns>Bitmapa zawierająca token</returns>
        public static System.Drawing.Image GetToken(out int width, out int height, out int length, out string id, out byte[] tokenData)
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
        #endregion

        #region Register
        /// <summary>
        /// Rejestruje nowy numer GG.
        /// </summary>
        /// <param name="password">Hasło</param>
        /// <param name="email">Email</param>
        /// <param name="tokenId">Indetyfikator tokenu</param>
        /// <param name="tokenValue">Wartość tokenu</param>
        /// <returns>Zarejestrowany numer GG. -1 jeśli podano złą wartość tokenu, -2 jeśli zbyt długie hasło, -3 jeśli zły adres e-mail, 0 w przypadku innego błędu</returns>
        public static uint Register(string password, string email, string tokenId, string tokenValue)
        {
            GGHTTPError ec;
            return Register(password, email, tokenId, tokenValue, out ec);
        }
        /// <summary>
        /// Rejestruje nowy numer GG.
        /// </summary>
        /// <param name="password">Hasło</param>
        /// <param name="email">Email</param>
        /// <param name="tokenId">Indetyfikator tokenu</param>
        /// <param name="tokenValue">Wartość tokenu</param>
        /// <param name="errorCode">Kod błędu</param>
        /// <returns>Zarejestrowany numer GG. -1 jeśli podano złą wartość tokenu, -2 jeśli zbyt długie hasło, -3 jeśli zły adres e-mail, 0 w przypadku innego błędu</returns>
        public static uint Register(string password, string email, string tokenId, string tokenValue, out GGHTTPError errorCode)
        {
            string query = string.Format("?pwd={0}&email={1}&tokenid={2}&tokenval={3}&code={4}", password, email, tokenId, tokenValue, Hash(new string[] { email, password }).ToString());
            string resp = StringRequest("http://register.gadu-gadu.pl/fmregister.php" + query, "POST", false);

            if (resp.StartsWith("reg_success"))
            {
                string[] splitted = resp.Split(':');
                if (splitted.Length >= 2)
                {
                    errorCode = GGHTTPError.Success;
                    return uint.Parse(splitted[1]);
                }
                else
                {
                    errorCode = GGHTTPError.None;
                    return 0;
                }
            }
            else if (resp.StartsWith("bad_tokenval"))
            {
                errorCode = GGHTTPError.BadTokenValue;
                return -1;
            }
            else if (resp.StartsWith("error3"))
            {
                errorCode = GGHTTPError.PasswordTooLong;
                return -2;
            }
            else if (resp == string.Empty)
            {
                errorCode = GGHTTPError.BadEmail;
                return -3;
            }
            else
            {
                errorCode = GGHTTPError.None;
                return 0;
            }
        }
        #endregion

        #region Password
        /// <summary>
        /// Zmienia hasło do konta GG.
        /// </summary>
        /// <param name="uin">Numer GG</param>
        /// <param name="oldPassword">Stare hasło</param>
        /// <param name="newPassword">Nowe hasło</param>
        /// <param name="email">Email</param>
        /// <param name="tokenId">Indetyfikator tokenu</param>
        /// <param name="tokenValue">Wartość tokenu</param>
        /// <returns>Kod błędu</returns>
        public static GGHTTPError ChangePassword(uint uin, string oldPassword, string newPassword, string email, string tokenId, string tokenValue)
        {
            string query = string.Format("?fmnumber={0}&fmpwd={1}&pwd={2}&email={3}&tokenid={4}&tokenval={5}&code={6}", uin.ToString(), oldPassword, newPassword, email, tokenId, tokenValue, Hash(new string[] { email, newPassword }));
            string resp = StringRequest("http://register.gadu-gadu.pl/fmregister.php" + query, "POST", false);

            if (resp.StartsWith("reg_success"))
                return GGHTTPError.Success;
            else if (resp.StartsWith("not authenticated"))
                return GGHTTPError.BadOldPassword;
            else if (resp.StartsWith("error1"))
                return GGHTTPError.PasswordTooLong;
            else if (resp.StartsWith("bad_tokenval"))
                return GGHTTPError.BadTokenValue;
            else if (resp == string.Empty)
                return GGHTTPError.BadEmail;
            else return GGHTTPError.None;
        }

        /// <summary>
        /// Przypomina hasło do konta GG.
        /// Nie przetestowane, mogą być problemy.
        /// </summary>
        /// <param name="uin">Numer GG</param>
        /// <param name="tokenId">Indetyfikator tokenu</param>
        /// <param name="tokenValue">Wartość tokenu</param>
        /// <returns>Kod błędu</returns>
        public static GGHTTPError RemindPassword(uint uin, string tokenId, string tokenValue)
        {
            string query = string.Format("?userid={0}&tokenid={1}&tokenval={2}&code={3}", uin.ToString(), tokenId, tokenValue, Hash(new string[] { uin.ToString() }));
            string resp = StringRequest("http://register.gadu-gadu.pl/fmsendpwd.php" + query, "POST", false);

            if (resp.StartsWith("pwdsend_success"))
                return GGHTTPError.Success;
            else return GGHTTPError.PasswordRemindFailed;
        }
        #endregion
    }
}
