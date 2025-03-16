using CinemaProject.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace CinemaProject.Controllers
{
    public class HomeController : Controller
    {
        //Database
        private readonly CinemaDB db;

        public HomeController(CinemaDB db)
        {
            this.db = db;
        }
        //To Save Login 
        public static CookieOptions options;
        //To Check Login 
        public string Check()
        {
            UsersDB User = new UsersDB();
            try
            {
                User.UserId = int.Parse(Request.Cookies["UserID"]);
                User.UserName = Request.Cookies["UserName"];
                User.Email = Request.Cookies["UserEmail"];
                User.PhoneNumber = Request.Cookies["UserPhone"];
                User.RulesID = int.Parse(Request.Cookies["UserRule"]);
                ViewBag.RuleName = Request.Cookies["RulesName"];
            }
            catch
            {
            }
            if (!string.IsNullOrEmpty(User.Email) && !string.IsNullOrEmpty(User.UserName) && !string.IsNullOrEmpty(User.PhoneNumber) && User.UserId != 0)
                return "1";//Success Login
            else
                return "0";//Failed Login
        }
        ////////////////IndexPage////////////////////////
        public IActionResult Index()
        {
            if (Check()=="1")
                return RedirectToAction("Index", "User");
            else
                return View();
        }

        ///////////////Login Page/////////////////////
        [HttpGet]
        public IActionResult Login()
        {
            if(Check()=="0")
                return View();
            else
                return RedirectToAction("Index", "User");
        }
        ///////////////Login Page
        [HttpPost]
        public IActionResult Login(Login Login)
        {
            var login = db.Users.ToArray();
            foreach (var user in login)
            {
                if ((user.UserName == Login.UName || user.PhoneNumber == Login.UName || user.Email == Login.UName) && user.Password == Login.Password)
                {
					options = new CookieOptions
					{
						Expires = DateTime.Now.AddDays(7) // صلاحية الكوكيز لمدة 7 أيام
					};
					Response.Cookies.Append("UserID", user.UserId.ToString(), options);
                    Response.Cookies.Append("UserName", user.UserName, options);
                    Response.Cookies.Append("UserEmail", user.Email, options);
                    Response.Cookies.Append("UserPhone", user.PhoneNumber, options);
                    Response.Cookies.Append("UserRule", user.RulesID.ToString(), options);
                    var Rules = db.Rules.ToArray();
                    foreach (var item in Rules)
                    {
                        if (item.RulesID == user.RulesID)
                        {
                            Response.Cookies.Append("RulesName", item.RulesName, options);
                            break;
                        }
                    }
                    return RedirectToAction("Index", "User");
                }
            }
            ViewBag.ErrorMessageUser = "The UserName or Password not found";
            return View();
        }
        ///////////////Sign-Up Page/////////////////////
        [HttpGet]
        public IActionResult Signup()
        {
            if (Check() == "0")
                return View();
            else
                return RedirectToAction("Index", "User");
        }
        ///////////////Sign-Up Page
        [HttpPost]
        public IActionResult Signup(UsersDB New)
        {
            var User = db.Users.ToList();
            if (New.PhoneNumber[0] == '0' && New.PhoneNumber[1] == '1' && New.PhoneNumber.Length == 11)
            {
                foreach (var user in User)
                {
                    if (user.Email == New.Email || user.PhoneNumber == New.PhoneNumber)
                    {
                        ViewBag.ErrorMessageUser = "This Email or Phone Number was found";
                        return View();
                    }
                }
                var newU = new UsersDB { UserName = New.UserName, PhoneNumber = New.PhoneNumber, Email = New.Email, Password = New.Password, RulesID = 2 };
                db.Users.Add(newU);
                db.SaveChanges();
                return View("Login");
            }
            else
            {
                ViewBag.ErrorMessageUser = "The Phone Number must start with 01 and length 11 number";
                return View();
            }
        }
        /////////////////////////////////////
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}