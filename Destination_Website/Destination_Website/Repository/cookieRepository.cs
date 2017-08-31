using System;
using System.Web;

namespace Destination_Website.Repository
{
    public class cookieRepository
    {
        public string cookieName = "QRcode_LogIn";
        public void NewCookie()
        {
            HttpCookie cookie = new HttpCookie(cookieName);
            cookie.Expires = DateTime.Now.AddYears(1);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public bool IsCookieExist(HttpCookie cookie)
        {
            if (cookie != null) return true;
            else return false;
        }

        public string GetCookieGuid(HttpCookie cookie)
        {
            if (cookie != null) return cookie["guid"];
            else return null;
        }

        public string SetCookieGuid(HttpCookie cookie, string userName)
        {
            if (cookie != null)
            {
                var validate = new validateRepository();
                var newGuid = validate.GenerateGuid().ToString();
                _SetCookieValue(cookie, "guid", newGuid);
                var guid = GetCookieGuid(cookie);
                if (guid == null) return "Something Wrong when get GUID!";
                else
                {
                    var conn = new UserRepository();
                    conn.SetGuid(userName, guid);
                    return "Finish set Guid!";
                }
            }
            else return "No cookie";
        }


        private void _SetCookieValue(HttpCookie cookie, string newKey, string newValue)
        {
            cookie.Values.Set(newKey, newValue);
            HttpContext.Current.Response.AppendCookie(cookie);
        }

        private void _ModifyCookie(ref HttpCookie cookie, string key, string newValue)
        {
            if (cookie != null)
            {
                cookie.Values[key] = newValue;
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        public string ModifyCookieGuid(HttpCookie cookie)
        {
            if (cookie != null)
            {
                var validate = new validateRepository();
                var newGuid = validate.GenerateGuid().ToString();
                _ModifyCookie(ref cookie, "guid", newGuid);
                return "Finish modify Guid";
            }
            else return "No cookie";
        }

        /// <summary>
        /// delete a key in the cookie(with parameter)
        /// or whole cookie(without parameter)
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="key"></param>
        public void DeleteCookie(HttpCookie cookie, string key = "whole")
        {
            if (cookie != null)
            {
                if (key == "whole")
                {
                    HttpCookie cookieAfter = new HttpCookie(cookieName);
                    cookieAfter.Expires = DateTime.Now.AddDays(-1d);
                    HttpContext.Current.Response.Cookies.Add(cookieAfter);
                }
                else
                {
                    cookie.Values.Remove(key);
                    HttpContext.Current.Response.Cookies.Add(cookie);
                }

            }
        }

    }
}