using JotForm;
using MYOB.AccountRight.SDK;
using MYOB.AccountRight.SDK.Contracts;
using MYOB.AccountRight.SDK.Services;
using MyObJsonDeserialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Xml;

namespace EnquiryInsertToCRM.Models
{

    public class CommonMethod
    {
        public static IApiConfiguration _configurationCloud;
        public static IOAuthKeyService _oAuthKeyService;
        public const string iOAuthKeyService = "MyOAuthKeyService";
        public static string TimeZoneKey = "AUS Eastern Standard Time";
        //public static string TimeZoneKey = "India Standard Time";

        public static DateTime GetUserTimeZoneDateTime(string strUtcDateTime)
        {
            DateTime retVal;

            DateTime utcDateTime = Convert.ToDateTime(strUtcDateTime);

            string userTimeZoneKey = TimeZoneKey; //"India Standard Time";
            TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneKey);
            retVal = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, userTimeZone);

            return retVal;
        }

        public static DateTime GetDateToUTCDateTime(string strDateTime)
        {
            DateTime retVal;

            DateTime dateTime = Convert.ToDateTime(strDateTime);

            string Date = dateTime.ToString("dd/MM/yyyy HH:mm:ss");
            //string Time = DateTime.Now.ToString("HH:mm:ss");
            //string Time = GetUserTimeZoneDateTime(Convert.ToString(DateTime.UtcNow)).ToString("HH:mm:ss");
            //string date = Date + " " + Time;
            TimeSpan timeDiffWithUTC = GetTimeDiffwithUtc();

            //retVal = Convert.ToDateTime(date).ToUniversalTime();
            retVal = Convert.ToDateTime(Date).AddMinutes(0 + timeDiffWithUTC.TotalMinutes);

            return retVal;
        }

        public static TimeSpan GetTimeDiffwithUtc()
        {
            string userTimeZoneKey = TimeZoneKey;
            TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneKey);
            TimeSpan offset;
            offset = userTimeZone.BaseUtcOffset;

            return offset;
        }

        #region Mail Send
        public static string MailSend(string strMailTo, string strMailSubject, string strMailBody, bool blnAllowMultipleBCC, bool sendBCC, bool? blnSendToAdmin)
        {
            string res = "";
            try
            {
                string emailFrom = ConfigurationManager.AppSettings["emailFrom"];
                string sitename = ConfigurationManager.AppSettings["sitename"];
                string strBCCEmail = ConfigurationManager.AppSettings["bccEmail"];
                string strMultipleBCCEmail = ConfigurationManager.AppSettings["MultipleBCCEmail"];
                string smtpServer = ConfigurationManager.AppSettings["smtpServer"];
                int? smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["smtpPort"]);
                bool smtpEnableSSL = Convert.ToBoolean(ConfigurationManager.AppSettings["smtpEnableSSL"]);
                string smtpAuthEmail = ConfigurationManager.AppSettings["smtpAuthEmail"];
                string smtpAuthPassword = ConfigurationManager.AppSettings["smtpAuthPassword"];
                string strSupportMailId = ConfigurationManager.AppSettings["SupportMailId"];

                string strCopyrightyear = DateTime.Now.Year.ToString();

                if ((blnSendToAdmin ?? false))
                {
                    strMailTo = strSupportMailId;
                }

                if (!string.IsNullOrWhiteSpace(emailFrom))
                {
                    MailMessage MyMailMessage = new MailMessage();
                    MyMailMessage.From = new MailAddress(emailFrom, sitename);
                    string[] strMailToList = strMailTo.Split(',');
                    if (strMailToList != null && strMailToList.Length > 0)
                    {
                        foreach (string strItem in strMailToList)
                        {
                            MyMailMessage.To.Add(strItem);
                        }
                    }
                    string[] strBCCEmails = strMultipleBCCEmail.Split(',');
                    if (sendBCC == true)
                    {
                        if (strBCCEmails != null && strBCCEmails.Length > 0)
                        {
                            strBCCEmail = strBCCEmails[0].Trim();
                        }
                        if (blnAllowMultipleBCC == true)
                        {
                            if (!string.IsNullOrEmpty(strMultipleBCCEmail))
                            {
                                strBCCEmails = strMultipleBCCEmail.Split(',');
                                if (strBCCEmails != null && strBCCEmails.Length > 0)
                                {
                                    foreach (string strItem in strBCCEmails)
                                    {
                                        MyMailMessage.CC.Add(strItem.Trim());
                                    }
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(strBCCEmail))
                                {
                                    MyMailMessage.CC.Add(strBCCEmail);
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(strBCCEmail))
                            {
                                MyMailMessage.CC.Add(strBCCEmail);
                            }
                        }
                    }
                    MyMailMessage.Subject = strMailSubject;
                    MyMailMessage.IsBodyHtml = true;
                    MyMailMessage.Body = strMailBody;
                    SmtpClient SMTPServer = new SmtpClient(smtpServer);
                    SMTPServer.UseDefaultCredentials = false;
                    SMTPServer.Timeout = 10000;
                    SMTPServer.Port = Convert.ToInt32(smtpPort);
                    SMTPServer.EnableSsl = smtpEnableSSL;
                    SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                    SMTPServer.Credentials = new NetworkCredential(smtpAuthEmail, smtpAuthPassword);
                    SMTPServer.Send(MyMailMessage);
                    res = "success";
                }
                return res;
            }
            catch
            {
                return res = "failed";
            }
        }
        #endregion

        #region Create Log
        public static void LogFile(StringBuilder textWrite, bool isInitial)
        {
            var filename = "~/images/WebHook_Response5.txt";
            if (!File.Exists(HttpContext.Current.Server.MapPath(filename)))
            {
                File.Create(HttpContext.Current.Server.MapPath(filename));
            }
            using (StreamWriter sw = File.AppendText(HttpContext.Current.Server.MapPath(filename)))
            {
                if (isInitial)
                {
                    sw.WriteLine("===========================================================================================");
                    sw.WriteLine("------------------------------Request: " + DateTime.UtcNow + "--------------------------------\n");
                }
                sw.WriteLine(textWrite);
                sw.Dispose();
            }
        }
        public static void LogFile_PK(StringBuilder textWrite, bool isInitial)
        {
            var filename = "~/images/Response_PK.txt";
            if (!File.Exists(HttpContext.Current.Server.MapPath(filename)))
            {
                File.Create(HttpContext.Current.Server.MapPath(filename));
            }
            using (StreamWriter sw = File.AppendText(HttpContext.Current.Server.MapPath(filename)))
            {
                if (isInitial)
                {
                    sw.WriteLine("===========================================================================================");
                    sw.WriteLine("------------------------------Request: " + DateTime.Now + "--------------------------------\n");
                }
                sw.WriteLine(textWrite);
                sw.Dispose();
            }
        }
        #endregion

        #region Schedular Create Log
        public static void SchedularLogFile(StringBuilder textWrite, bool isInitial)
        {
            var filename = @"C:\inetpub\vhosts\modelbuzz.in\httpdocs\btc\images\Schedular.txt";
            //var filename = @"D:\Manoj\Projects\EnquiryInsertToCRM\EnquiryInsertToCRM\images\Schedular.txt";
            if (!System.IO.File.Exists(filename))
            {
                System.IO.File.Create(filename);
            }
            using (StreamWriter sw = System.IO.File.AppendText(filename))
            {
                if (true)
                {
                    sw.WriteLine("===========================================================================================");
                    sw.WriteLine("------------------------------Request: " + DateTime.Now + "--------------------------------\n");
                }
                sw.WriteLine(textWrite);
                sw.Dispose();
            }
        }
        #endregion

        #region Create Log
        public static void LogFileForLastSaleDate(StringBuilder textWrite, bool isInitial)
        {
            var filename = "/myobsync.txt";
            if (!File.Exists(HttpContext.Current.Server.MapPath(filename)))
            {
                File.Create(HttpContext.Current.Server.MapPath(filename)).Close();
            }
            using (StreamWriter sw = File.AppendText(HttpContext.Current.Server.MapPath(filename)))
            {
                if (isInitial)
                {
                    sw.WriteLine("------------------------------Sync Date: " + DateTime.Now + "--------------------------------\n");
                }
                sw.WriteLine(textWrite);
                sw.WriteLine(" ");
                sw.Dispose();
            }
        }
        #endregion

        #region Make Api Call

        public static JArray MakeAccountRightAPICall(string loURL, string token, string key, string cftoken, string filter = "")
        {
            //loURL = "https://ar1.api.myob.com/accountright/6185379c-2a07-4d75-bcd5-ca2ede7a5717/Contact/Customer/?$top=400";
            if (!string.IsNullOrEmpty(filter))
            {
                loURL = loURL + filter;
            }
            JArray lstSaleInvoice = new JArray();
            string NextPageLink = "";
            HttpWebRequest loHttpWebRequest = (HttpWebRequest)WebRequest.Create(loURL);
            loHttpWebRequest.Method = "GET";
            loHttpWebRequest.ContentType = "appication/json";
            loHttpWebRequest.Headers.Add("Authorization", "Bearer " + token + "");
            loHttpWebRequest.Headers.Add("x-myobapi-key", key);
            loHttpWebRequest.Headers.Add("x-myobapi-version", "v2");
            if (!string.IsNullOrEmpty(cftoken))
            {
                loHttpWebRequest.Headers.Add("x-myobapi-cftoken", cftoken);
            }

            try
            {
                System.IO.Stream response = null;
                HttpWebResponse loHttpWebResponse = (HttpWebResponse)loHttpWebRequest.GetResponse();
                //StreamReader loStreamReader = new StreamReader(loHttpWebResponse.GetResponseStream());
                response = loHttpWebResponse.GetResponseStream();
                JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());
                if (responseJSON != null)
                {
                    lstSaleInvoice = responseJSON["Items"].Value<JArray>();
                    NextPageLink = responseJSON["NextPageLink"].Value<string>();
                    if (!string.IsNullOrEmpty(NextPageLink ?? ""))
                    {
                        var arrayResult = MakeAccountRightAPICall(NextPageLink, token, key, cftoken, "");
                        if (arrayResult != null && arrayResult.Count > 0)
                        {
                            lstSaleInvoice.Merge(arrayResult);
                        }
                        //LogFile(sb, false);
                    }
                }
            }

            catch (Exception ex)
            {
                LogFile(new StringBuilder("#4 MakeAccountRightAPICall Step Error " + ex.Message), false);
            }
            return lstSaleInvoice;
        }
        public static JToken MakeAccountRightAPICall_SingleItemReturn(string loURL, string token, string key, string cftoken)
        {
            HttpWebRequest loHttpWebRequest = (HttpWebRequest)WebRequest.Create(loURL);
            loHttpWebRequest.Method = "GET";
            loHttpWebRequest.ContentType = "appication/json";
            loHttpWebRequest.Headers.Add("Authorization", "Bearer " + token + "");
            loHttpWebRequest.Headers.Add("x-myobapi-key", key);
            loHttpWebRequest.Headers.Add("x-myobapi-version", "v2");
            if (!string.IsNullOrEmpty(cftoken))
            {
                loHttpWebRequest.Headers.Add("x-myobapi-cftoken", cftoken);
            }

            try
            {
                System.IO.Stream response = null;
                HttpWebResponse loHttpWebResponse = (HttpWebResponse)loHttpWebRequest.GetResponse();
                response = loHttpWebResponse.GetResponseStream();
                JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());
                if (responseJSON != null)
                {
                    return responseJSON;
                }
            }
            catch (Exception ex)
            {
                LogFile(new StringBuilder("#4 MakeAccountRightAPICall_SingleItemReturn Step Error " + ex.Message), false);
            }
            return null;
        }
        #endregion

        #region Encode String
        public static string GetEncode(string plainText)
        {

            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            string encodedText = Convert.ToBase64String(plainTextBytes);

            return encodedText;
        }
        #endregion

        public static void ReadFromFile(OAuthTokens _tokens)
        {

            string CsTokensFile = ConfigurationManager.AppSettings["TokensFilePath"];

            //OAuthTokens _tokens = new OAuthTokens();
            //_tokens.AccessToken = "jhgfhdx";

            //var isoStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(CsTokensFile), FileMode.Open, FileAccess.Read);
            //isoStream.SetLength(0);
            if (File.Exists(System.Web.HttpContext.Current.Server.MapPath(CsTokensFile)))
            {

                File.Delete(System.Web.HttpContext.Current.Server.MapPath(CsTokensFile));
            }
            using (StreamWriter sw = File.CreateText(System.Web.HttpContext.Current.Server.MapPath(CsTokensFile)))
            {
                sw.Write(JsonConvert.SerializeObject(_tokens));
                sw.Dispose();
            }

        }

        public static bool IsAllDigits(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        public static bool IsExpiredToken_BasedOnCookies()
        {
            if (HttpContext.Current.Request.Cookies["AccessToken"] == null)
            {
                return true;
            }
            else
            {
                HttpCookie cookie_AccessToken = HttpContext.Current.Request.Cookies["AccessToken"];
                HttpCookie cookie_RefreshToken = HttpContext.Current.Request.Cookies["RefreshToken"];
                HttpCookie cookie_ReceivedTime = HttpContext.Current.Request.Cookies["ReceivedTime"];
                DateTime a = Convert.ToDateTime(cookie_ReceivedTime.Value);
                DateTime b = DateTime.UtcNow;
                double totalDiff = b.Subtract(a).TotalMinutes;
                if (totalDiff > 18)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public static bool IsExpiredTokenForPk_BasedOnCookies()
        {
            if (HttpContext.Current.Request.Cookies["AccessToken_Pk"] == null)
            {
                return true;
            }
            else
            {
                HttpCookie cookie_AccessToken = HttpContext.Current.Request.Cookies["AccessToken_Pk"];
                HttpCookie cookie_RefreshToken = HttpContext.Current.Request.Cookies["RefreshToken_Pk"];
                HttpCookie cookie_ReceivedTime = HttpContext.Current.Request.Cookies["ReceivedTime_Pk"];
                DateTime a = Convert.ToDateTime(cookie_ReceivedTime.Value);
                DateTime b = DateTime.UtcNow;
                double totalDiff = b.Subtract(a).TotalMinutes;
                if (totalDiff > 18)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public static bool IsExpiredToken()
        {
            _oAuthKeyService = (IOAuthKeyService)HttpContext.Current.Session[iOAuthKeyService];
            if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetAccessToken()
        {
            if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("localhost"))
            {
                return "AAEAAGyLIVqt0uKuCrWCcA9GpAtOxYL4OltUgeygyf5Qgc1mNJyzVXAxcTjBHvll5FskvMIpen5sbufcUGNj24J5NQB4cTU8BOQIKH0thd4i5XQBJeZr3ugF7gpOFnGSAxQri-7Uj-rm29OK06LfEK0nFSHzOCz5ThlCLeN3Zi_YLw1v-3XteaTgvUMBVPoXKVYG_IrbFwCwcp16rFHbbupRb7hc5oj7c1XdyrjRYcW8K7V7pNdbjZeTCSZgiGaxYyvU4PPWNhnAIvHrcHiSoAW_vduE5PpnLaLvy6ZfFxsxQvx_bc3SKnHM80M1QPLEEbtRA8QXisX6X4D_s7i3j76OLLqkAQAAAAEAAFHW7kzqkwEYtxgqlc0nxtuiOLvJmbrEnSLQXw1EBHHg-A7Uk5YgrP0WVQUfAG5qROuo5Gulv3wW8A-09hVfa8veLVOaFc3adgMEg1_uIRA497vjGubiKb2mzHf0xKOoZ0pwKeHo72zMfClygClahGRZXDVpnav0vLXKunxA1-ttjd55FtMnlqZ-hvqr_2WxjA1mkRru3zXtQNs5ibGkkaHibdY7uQpWougFp4MhuvG_TAq-vjYoLDb1uusn9_d0IyX1zv3LRiVvNyWcAKD9fO_zAAbM2enASG5AERxX1bolMxREzHOEtRFYOXPGeuzFLAyXoTllu9g6GEKVBGC06HzSNnfCwVIX93cynP3mSKYnt7Ju4STnwCaMxufBMjzIHqy7j4prRuFRvgLLtNPHyjN9oyRS7J-8XCCgHWNKqZnBLEIaADwRGNLDFh3Evyn-5Elje4zpw9uODyhvgx7HtCuVH66oTEei3q5U2YI8Nv_3Yeak6LdT8kbdWzfuIP1XeIZlZzZt9SwVAs7Fq8nmFHovB0Nu2oVzvCkaOOM_8eyG";
            }
            HttpCookie cookie_AccessToken = HttpContext.Current.Request.Cookies["AccessToken"];
            HttpCookie cookie_RefreshToken = HttpContext.Current.Request.Cookies["RefreshToken"];
            HttpCookie cookie_ReceivedTime = HttpContext.Current.Request.Cookies["ReceivedTime"];
            DateTime a = Convert.ToDateTime(cookie_ReceivedTime.Value);
            DateTime b = DateTime.UtcNow;

            return cookie_AccessToken.Value;
            //return " AccessToken: " + cookie_AccessToken.Value + " ,RefreshToken: " + cookie_RefreshToken.Value + " ,ReceivedTime: " + cookie_ReceivedTime.Value + " , Current Time: " + DateTime.UtcNow + " ,TimeDiff: " + b.Subtract(a).TotalMinutes;
        }
        public static string GetAccessToken_Pk()
        {
            if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("localhost"))
            {
                return "AAEAAFl6A2Po5__jAEJXkjZnY4JPwvLZzzOG1TEvHSS0OgXvgPf8pyKNrJn9H_q_dqCrmu-w0RYHs94SJW5U2NVRRZBsy-404Q3v0gKRg-_ksTER3qNPCXx4_R8eTylmFKgglPYlGtyLpL_0Xuk22Tar4BYBLmYIZTmakXB2_mM3sM_465C883Z_Skj3T7dxj-AVpmlWPCUdqB7MwrPe7WkqhFNAFDawVRDFGhn56AGf2JxM_ca6Agy_nXoxTNtQXr0Kd-HELnrm5MZt3-Nq0gFNypzhjiblNnpKBhqXBeeV8_B32EpA9aLvrbsYhX8Tb9Z5fw4nspAD_cC2J0otcPNH-NCkAQAAAAEAAEhkG6eqTPgF4fpae1zlk4IJKXBFktOyq5KchUNY6Qz_tDNVY8IFDD3ntcIyQTArLAUUP-zMw5RoxcRIDeAOPnVYlGeFl2deyjeuP2BbGAEHziTIm44HcH1vQN3W50hDZzOhMILIYRoBbci8xC538X5RpZID4lfr3yqRc8-aBmW-GuRHlhdFgpdmjLYbYt8d7u_cTwGhToxoEnG_hfU9wWlzD4Ku-hKy6PW1UC2wwZ-uB_YQ8kVvvLxqMO4UZvd-erxYBK-hpOQzntJrimgheEUNcoDh06gIwm15HjcE4IEQyIglUJem2v7d5K7okIXRAC8VPLbopUhk5JTaZUB1Y9itcaw6_8MmsZu356yYdcze0QJZbdO6RD1j9r87j5XuXIUfLN4JSI9TynU-JMSS2rtWuK6qtC8EWrrYPtGNOckQH1UTjQW9FrxNaBNHQbG2B5DHIlqTaG_EthG__cXl3Cm_8e2s8KtMfvBoJYSw17_LaDOQjVlbZP20iyvdgxww8k9LlnQBGeCVSGbKrgzpMosQ0sK2HHsZTSJXfyVQ94CF";
            }
            HttpCookie cookie_AccessToken = HttpContext.Current.Request.Cookies["AccessToken_Pk"];
            HttpCookie cookie_RefreshToken = HttpContext.Current.Request.Cookies["RefreshToken_Pk"];
            HttpCookie cookie_ReceivedTime = HttpContext.Current.Request.Cookies["ReceivedTime_Pk"];
            DateTime a = Convert.ToDateTime(cookie_ReceivedTime.Value);
            DateTime b = DateTime.UtcNow;

            return cookie_AccessToken.Value;
            //return " AccessToken: " + cookie_AccessToken.Value + " ,RefreshToken: " + cookie_RefreshToken.Value + " ,ReceivedTime: " + cookie_ReceivedTime.Value + " , Current Time: " + DateTime.UtcNow + " ,TimeDiff: " + b.Subtract(a).TotalMinutes;
        }

        public static IOAuthKeyService RefreshToken()
        {

            StringBuilder sb = new StringBuilder();

            string redirect_uri = ConfigurationManager.AppSettings["SyncMyOb2CRM_RedirectUri"];
            string client_id = ConfigurationManager.AppSettings["DeveloperKey"];
            string client_secret = ConfigurationManager.AppSettings["DeveloperSecret"];
            string TokensFilePath = ConfigurationManager.AppSettings["TokensFilePath"];
            string XMLFilePath = ConfigurationManager.AppSettings["XMLFilePath"];
            string myobCompany = ConfigurationManager.AppSettings["MyObCompany"];
            int expirein = Convert.ToInt32(ConfigurationManager.AppSettings["expirein"]);

            _oAuthKeyService = (IOAuthKeyService)HttpContext.Current.Session[iOAuthKeyService];
            _configurationCloud = new ApiConfiguration(client_id, client_secret, redirect_uri);
            var oauthService = new OAuthService(_configurationCloud);
            _oAuthKeyService.OAuthResponse = oauthService.RenewTokens(_oAuthKeyService.OAuthResponse);

            sb.AppendLine("========RefreshToken=============");
            sb.AppendLine("RefreshToken Current Expired time: " + _oAuthKeyService.OAuthResponse.ReceivedTime + "");
            sb.AppendLine("RefreshToken Current ExpiresIn : " + _oAuthKeyService.OAuthResponse.ExpiresIn);
            //_oAuthKeyService.OAuthResponse.ReceivedTime = _oAuthKeyService.OAuthResponse.ReceivedTime.AddMinutes(-5);
            sb.AppendLine("RefreshToken AccessToken : " + _oAuthKeyService.OAuthResponse.AccessToken);
            _oAuthKeyService.OAuthResponse.ExpiresIn = expirein;
            sb.AppendLine("RefreshToken New  ExpiresIn: " + _oAuthKeyService.OAuthResponse.ExpiresIn + "");
            sb.AppendLine("RefreshToken New Expired time: " + _oAuthKeyService.OAuthResponse.ReceivedTime + "");
            sb.AppendLine("=====================");
            HttpContext.Current.Session[iOAuthKeyService] = _oAuthKeyService;
            //System.Web.HttpCookie AccessTokenCookie = new System.Web.HttpCookie("AccessToken");
            //System.Web.HttpCookie RefreshTokenCookie = new System.Web.HttpCookie("RefreshToken");
            //System.Web.HttpCookie ReceivedTimeCookie = new System.Web.HttpCookie("ReceivedTime");
            //HttpContext.Current.Request.Cookies.Clear();
            //HttpCookie cookie_AccessToken = new HttpCookie("AccessToken", _oAuthKeyService.OAuthResponse.AccessToken);
            //HttpCookie cookie_RefreshToken = new HttpCookie("RefreshToken", _oAuthKeyService.OAuthResponse.RefreshToken);
            //HttpCookie cookie_ReceivedTime = new HttpCookie("ReceivedTime", _oAuthKeyService.OAuthResponse.ReceivedTime.ToString());
            //cookie_AccessToken.Expires = DateTime.Now.AddHours(1);
            //cookie_RefreshToken.Expires = DateTime.Now.AddHours(1);
            //cookie_ReceivedTime.Expires = DateTime.Now.AddHours(1);
            //HttpContext.Current.Response.Cookies.Add(cookie_AccessToken);
            //HttpContext.Current.Response.Cookies.Add(cookie_RefreshToken);
            //HttpContext.Current.Response.Cookies.Add(cookie_ReceivedTime);

            //HttpContext.Current.Session.Timeout = 25;
            //CommonMethod.LogFile(sb, false);
            return _oAuthKeyService;
        }
        public static IOAuthKeyService RefreshToken_BasedOnCookies()
        {
            StringBuilder sb = new StringBuilder();
            _oAuthKeyService = new OAuthKeyService();
            _oAuthKeyService.OAuthResponse = new MYOB.AccountRight.SDK.Contracts.OAuthTokens();
            try
            {
                string redirect_uri = ConfigurationManager.AppSettings["SyncMyOb2CRM_RedirectUri"];
                string client_id = ConfigurationManager.AppSettings["DeveloperKey"];
                string client_secret = ConfigurationManager.AppSettings["DeveloperSecret"];
                string TokensFilePath = ConfigurationManager.AppSettings["TokensFilePath"];
                string XMLFilePath = ConfigurationManager.AppSettings["XMLFilePath"];
                string myobCompany = ConfigurationManager.AppSettings["MyObCompany"];
                int expirein = Convert.ToInt32(ConfigurationManager.AppSettings["expirein"]);

                HttpCookie cookie_AccessToken_old = HttpContext.Current.Request.Cookies["AccessToken"];
                HttpCookie cookie_RefreshToken_old = HttpContext.Current.Request.Cookies["RefreshToken"];
                HttpCookie cookie_ReceivedTime_old = HttpContext.Current.Request.Cookies["ReceivedTime"];

                _oAuthKeyService.OAuthResponse.AccessToken = cookie_AccessToken_old.Value;
                _oAuthKeyService.OAuthResponse.RefreshToken = cookie_RefreshToken_old.Value;
                _oAuthKeyService.OAuthResponse.ReceivedTime = Convert.ToDateTime(cookie_ReceivedTime_old.Value);
                _oAuthKeyService.OAuthResponse.ExpiresIn = 1200;

                _configurationCloud = new ApiConfiguration(client_id, client_secret, redirect_uri);
                var oauthService = new OAuthService(_configurationCloud);
                _oAuthKeyService.OAuthResponse = oauthService.RenewTokens(_oAuthKeyService.OAuthResponse);

                sb.AppendLine("========RefreshToken=============");
                sb.AppendLine("RefreshToken Current Expired time: " + _oAuthKeyService.OAuthResponse.ReceivedTime + "");
                sb.AppendLine("RefreshToken Current ExpiresIn : " + _oAuthKeyService.OAuthResponse.ExpiresIn);
                //_oAuthKeyService.OAuthResponse.ReceivedTime = _oAuthKeyService.OAuthResponse.ReceivedTime.AddMinutes(-5);
                sb.AppendLine("RefreshToken AccessToken : " + _oAuthKeyService.OAuthResponse.AccessToken);
                _oAuthKeyService.OAuthResponse.ExpiresIn = expirein;
                sb.AppendLine("RefreshToken New  ExpiresIn: " + _oAuthKeyService.OAuthResponse.ExpiresIn + "");
                sb.AppendLine("RefreshToken New Expired time: " + _oAuthKeyService.OAuthResponse.ReceivedTime + "");
                sb.AppendLine("=====================");
                //HttpContext.Current.Session[iOAuthKeyService] = _oAuthKeyService;
                System.Web.HttpCookie AccessTokenCookie = new System.Web.HttpCookie("AccessToken");
                System.Web.HttpCookie RefreshTokenCookie = new System.Web.HttpCookie("RefreshToken");
                System.Web.HttpCookie ReceivedTimeCookie = new System.Web.HttpCookie("ReceivedTime");
                HttpContext.Current.Request.Cookies.Clear();
                HttpCookie cookie_AccessToken = new HttpCookie("AccessToken", _oAuthKeyService.OAuthResponse.AccessToken);
                HttpCookie cookie_RefreshToken = new HttpCookie("RefreshToken", _oAuthKeyService.OAuthResponse.RefreshToken);
                HttpCookie cookie_ReceivedTime = new HttpCookie("ReceivedTime", _oAuthKeyService.OAuthResponse.ReceivedTime.ToString());
                cookie_AccessToken.Expires = DateTime.Now.AddHours(3);
                cookie_RefreshToken.Expires = DateTime.Now.AddHours(3);
                cookie_ReceivedTime.Expires = DateTime.Now.AddHours(3);
                HttpContext.Current.Response.Cookies.Add(cookie_AccessToken);
                HttpContext.Current.Response.Cookies.Add(cookie_RefreshToken);
                HttpContext.Current.Response.Cookies.Add(cookie_ReceivedTime);

                //HttpContext.Current.Session.Timeout = 25;
                //CommonMethod.LogFile(sb, false);
            }
            catch (Exception ex)
            {
                sb.AppendLine("Refresh Token Error:" + ex.Message);
                CommonMethod.LogFile(sb, false);
            }
            return _oAuthKeyService;
        }
        public static IOAuthKeyService RefreshTokenForPk_BasedOnCookies()
        {
            StringBuilder sb = new StringBuilder();
            _oAuthKeyService = new OAuthKeyService();
            _oAuthKeyService.OAuthResponse = new MYOB.AccountRight.SDK.Contracts.OAuthTokens();
            try
            {
                string redirect_uri = ConfigurationManager.AppSettings["SyncMyOb2CRM_RedirectUriForPK"];
                string client_id = ConfigurationManager.AppSettings["DeveloperKeyFoPK"];
                string client_secret = ConfigurationManager.AppSettings["DeveloperSecretFoPK"];

                int expirein = Convert.ToInt32(ConfigurationManager.AppSettings["expirein"]);

                HttpCookie cookie_AccessToken_Pk_old = HttpContext.Current.Request.Cookies["AccessToken_Pk"];
                HttpCookie cookie_RefreshToken_Pk_old = HttpContext.Current.Request.Cookies["RefreshToken_Pk"];
                HttpCookie cookie_ReceivedTime_Pk_old = HttpContext.Current.Request.Cookies["ReceivedTime_Pk"];

                _oAuthKeyService.OAuthResponse.AccessToken = cookie_AccessToken_Pk_old.Value;
                _oAuthKeyService.OAuthResponse.RefreshToken = cookie_RefreshToken_Pk_old.Value;
                _oAuthKeyService.OAuthResponse.ReceivedTime = Convert.ToDateTime(cookie_ReceivedTime_Pk_old.Value);
                _oAuthKeyService.OAuthResponse.ExpiresIn = 1200;

                _configurationCloud = new ApiConfiguration(client_id, client_secret, redirect_uri);
                var oauthService = new OAuthService(_configurationCloud);
                _oAuthKeyService.OAuthResponse = oauthService.RenewTokens(_oAuthKeyService.OAuthResponse);

                sb.AppendLine("========RefreshToken=============");
                sb.AppendLine("RefreshToken Current Expired time: " + _oAuthKeyService.OAuthResponse.ReceivedTime + "");
                sb.AppendLine("RefreshToken Current ExpiresIn : " + _oAuthKeyService.OAuthResponse.ExpiresIn);
                //_oAuthKeyService.OAuthResponse.ReceivedTime = _oAuthKeyService.OAuthResponse.ReceivedTime.AddMinutes(-5);
                sb.AppendLine("RefreshToken AccessToken : " + _oAuthKeyService.OAuthResponse.AccessToken);
                _oAuthKeyService.OAuthResponse.ExpiresIn = expirein;
                sb.AppendLine("RefreshToken New  ExpiresIn: " + _oAuthKeyService.OAuthResponse.ExpiresIn + "");
                sb.AppendLine("RefreshToken New Expired time: " + _oAuthKeyService.OAuthResponse.ReceivedTime + "");
                sb.AppendLine("=====================");
                //HttpContext.Current.Session[iOAuthKeyService] = _oAuthKeyService;
                System.Web.HttpCookie AccessTokenCookie = new System.Web.HttpCookie("AccessToken_Pk");
                System.Web.HttpCookie RefreshTokenCookie = new System.Web.HttpCookie("RefreshToken_Pk");
                System.Web.HttpCookie ReceivedTimeCookie = new System.Web.HttpCookie("ReceivedTime_Pk");
                HttpContext.Current.Request.Cookies.Clear();
                HttpCookie cookie_AccessToken_Pk = new HttpCookie("AccessToken_Pk", _oAuthKeyService.OAuthResponse.AccessToken);
                HttpCookie cookie_RefreshToken_Pk = new HttpCookie("RefreshToken_Pk", _oAuthKeyService.OAuthResponse.RefreshToken);
                HttpCookie cookie_ReceivedTime_Pk = new HttpCookie("ReceivedTime_Pk", _oAuthKeyService.OAuthResponse.ReceivedTime.ToString());
                cookie_AccessToken_Pk.Expires = DateTime.Now.AddHours(3);
                cookie_RefreshToken_Pk.Expires = DateTime.Now.AddHours(3);
                cookie_ReceivedTime_Pk.Expires = DateTime.Now.AddHours(3);
                HttpContext.Current.Response.Cookies.Add(cookie_AccessToken_Pk);
                HttpContext.Current.Response.Cookies.Add(cookie_RefreshToken_Pk);
                HttpContext.Current.Response.Cookies.Add(cookie_ReceivedTime_Pk);

                //HttpContext.Current.Session.Timeout = 25;
                CommonMethod.LogFile_PK(sb, false);
            }
            catch (Exception ex)
            {
                sb.AppendLine("Refresh Token Error:" + ex.Message);
                CommonMethod.LogFile(sb, false);
            }
            return _oAuthKeyService;
        }
        public static int TryParseInt32(string strData)
        {
            int intResult = 0;
            if (!string.IsNullOrEmpty(strData))
            {
                Int32.TryParse(strData, out intResult);
            }
            return intResult;
        }

        public static decimal TryParseDecimal(string strValue)
        {
            decimal res;
            decimal.TryParse(strValue, out res);
            return res;
        }
        public static IEnumerable<string> Split(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

        public static string GetCurrentFinancialYear()
        {
            int CurrentYear = DateTime.Today.Year;
            int PreviousYear = DateTime.Today.Year - 1;
            int NextYear = DateTime.Today.Year + 1;
            string PreYear = PreviousYear.ToString();
            string NexYear = NextYear.ToString();
            string CurYear = CurrentYear.ToString();
            string FinYear = null;

            if (DateTime.Today.Month > 3)
                FinYear = CurYear + "-" + NexYear;
            else
                FinYear = PreYear + "-" + CurYear;
            return FinYear.Trim();
        }


        public static int CurrentFinancialYear_newBup01072020(DateTime dateTime)
        {
            return dateTime.Month >= Convert.ToInt32(ConfigurationManager.AppSettings["FinancialYearStartDateMonth"]) ? dateTime.Year+1 : dateTime.Year;
        }
        public static int CurrentFinancialYear(DateTime dateTime)
        {
            return dateTime.Month >= Convert.ToInt32(ConfigurationManager.AppSettings["FinancialYearStartDateMonth"]) ? dateTime.Year + 1 : dateTime.Year;
        }
        public static DateTime GetFyStartDate(int Year)
        {
            DateTime dt = Convert.ToDateTime("01/" + Convert.ToInt32(ConfigurationManager.AppSettings["FinancialYearStartDateMonth"]) + "/" + Year).AddYears(-1);
            return dt;
        }
        public static DateTime GetFyEndDate(int Year)
        {
            DateTime dt = Convert.ToDateTime("30/" + (Convert.ToInt32(ConfigurationManager.AppSettings["FinancialYearEndDateMonth"])) + "/" + Year);
            return dt;
        }
        public static List<DataService.FinancialYearModel> GetFinancialYearList_Backup01072020()
        {
            List<DataService.FinancialYearModel> lstFyear = new List<DataService.FinancialYearModel>();
            int currentFy = CurrentFinancialYear(DateTime.Now);
            int fyStarting = currentFy - Convert.ToInt32(ConfigurationManager.AppSettings["FinancialYearExtend"]);
            for (int i = 0; i < Convert.ToInt32(ConfigurationManager.AppSettings["TotalFinancialYear"]); i++)
            {
                DataService.FinancialYearModel objModel = new DataService.FinancialYearModel();
                fyStarting += 1;
                objModel.fyStartDate = CommonMethod.GetFyStartDate(fyStarting);
                objModel.fyEndDate = CommonMethod.GetFyEndDate(fyStarting);
                //objModel.year = objModel.fyStartDate.Year;
                objModel.year = currentFy;
                objModel.key = ConfigurationManager.AppSettings["FinancialYearCRMFolder"] + "\\" + currentFy;
                if (currentFy == objModel.fyStartDate.Year)
                {
                    objModel.IsCurrentFY = true;
                }
                lstFyear.Add(objModel);
            }
            return lstFyear;
        }

        public static List<DataService.FinancialYearModel> GetFinancialYearList()
        {
            List<DataService.FinancialYearModel> lstFyear = new List<DataService.FinancialYearModel>();
            int currentFy = CurrentFinancialYear(DateTime.Now);
            int fyStarting = currentFy - Convert.ToInt32(ConfigurationManager.AppSettings["FinancialYearExtend"]);
            for (int i = 0; i < Convert.ToInt32(ConfigurationManager.AppSettings["TotalFinancialYear"]); i++)
            {
                DataService.FinancialYearModel objModel = new DataService.FinancialYearModel();
                fyStarting += 1;
                objModel.year = fyStarting;
                objModel.key = ConfigurationManager.AppSettings["FinancialYearCRMFolder"] + "\\" + fyStarting;
                objModel.fyStartDate = CommonMethod.GetFyStartDate(fyStarting);
                objModel.fyEndDate = CommonMethod.GetFyEndDate(fyStarting);
                if (currentFy == fyStarting)
                {
                    objModel.IsCurrentFY = true;
                }
                lstFyear.Add(objModel);
            }
            return lstFyear;
        }

        public static void SaveLogToTable(string strLog)
        {
            if (!string.IsNullOrWhiteSpace(strLog))
            {
                //using (DataServiceEntities _db = new DataServiceEntities())
                //{
                //    LogTable objLog = new LogTable();
                //    objLog.LogMessage = strLog;
                //    objLog.LogTime = DateTime.UtcNow;
                //    _db.LogTables.Add(objLog);
                //    _db.SaveChanges();
                //}
            }
        }
    }
}