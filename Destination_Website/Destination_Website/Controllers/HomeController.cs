using QRcodeTransfer;
using QRcodeTransfer.Repository;
using System;
using System.Web;
using System.Web.Mvc;
using Destination_Website.Models;

namespace Destination_Website.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index(string encode = null, string source = null)
        {
            var isUat = System.Configuration.ConfigurationManager.AppSettings["IsUAT"];
            string server_url;
            if (isUat.ToLower() == "y") server_url = System.Configuration.ConfigurationManager.AppSettings["computer_assign"];
            else server_url = "http://localhost:58065";
            ViewBag.url = server_url;

            if (source == "qr") return RedirectToAction("Validate_Identity", "Home", new { encode = encode, source = source });
            Session.Clear();
            return View();
        }

        [HttpPost]
        public ActionResult LogIn(string userName, string userPassword)
        {
            var conn = new UserRepository();
            var response = conn.UserLogin(userName, userPassword);
            if (response == "Login Success")
            {
                Session["UserName"] = userName;
            }
            return Json(response);
        }

        [HttpPost]
        public ActionResult Register(string userName, string userPassword)
        {
            var conn = new UserRepository();
            var response = conn.UserRegister(userName, userPassword);
            return Json(response);
        }

        [AuthorizeFilter]
        public ActionResult Murri_Index(string senderAction)
        {
            if (senderAction == "Validate_Button") ViewBag.tranferFlag = true;
            else ViewBag.tranferFlag = false;
            return View();
        }

        //[AuthorizeFilter]
        public ActionResult btnGoOnclick()
        {
            return View("GamePage");
        }

        public ActionResult Validate_Identity(string encode, string source = null)
        {

            if (source == "qr")
            {
                TempData["encode"] = encode.Replace(" ", "+");
                var key = System.Configuration.ConfigurationManager.AppSettings["aes_Key"];
                var userName = AesAction.Decryption(encode, key);

                var conn = new UserRepository();
                var check_url_enable = conn.GetUrlStatus(encode.Replace(" ", "+"));
                if (check_url_enable == 1) return RedirectToAction("WronguserPage", "Home", new { userName = userName });

                var response = conn.UserValidatefromQRcode(userName);
                if (response == "Login Success")
                {
                    conn.Update_enable(encode.Replace(" ", "+"));
                    Session["UserName"] = userName;
                    return RedirectToAction("WaitPage", "Home");
                }
                else return Content(response);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult WronguserPage(string userName)
        {
            var isUat = System.Configuration.ConfigurationManager.AppSettings["IsUAT"];
            string server_url;
            if (isUat.ToLower() == "y") server_url = System.Configuration.ConfigurationManager.AppSettings["AUrl_Hub"];
            else server_url = "http://localhost:58065/signalr";
            ViewBag.url = server_url;
            ViewBag.userName = userName;
            return View();
        }

        public ActionResult Validate_Button(string receiveCode, string encode)
        {
            var action = (string)RouteData.Values["action"];
            return RedirectToAction("Murri_Index", "Home", new { senderAction = action });
        }

        [AuthorizeFilter]
        public ActionResult WaitPage()
        {
            var isUat = System.Configuration.ConfigurationManager.AppSettings["IsUAT"];
            string server_url;
            string computer_assign_url;

            if (isUat.ToLower() == "y") { 
                server_url = System.Configuration.ConfigurationManager.AppSettings["AUrl_Hub"];
                computer_assign_url = System.Configuration.ConfigurationManager.AppSettings["computer_assign"];
            }
            else {
                server_url = "http://localhost:58065/signalr";
                computer_assign_url = "http://localhost:58065";
            }

            ViewBag.url = server_url;
            ViewBag.computer_url = computer_assign_url;
            return View();
        }
    }

}
