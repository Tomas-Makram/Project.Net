using CinemaProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaProject.Controllers
{
    public class UserController : Controller
    {
        //Database
        private readonly CinemaDB db;

        public UserController(CinemaDB db)
        {
            this.db = db;
        }

        //To Check Login
        public bool Check()
        {
            UsersDB User = new UsersDB();
            try
            {
                User.UserId = int.Parse(Request.Cookies["UserID"]);
                User.UserName = Request.Cookies["UserName"];
                User.Email = Request.Cookies["UserEmail"];
                User.PhoneNumber = Request.Cookies["UserPhone"];
                User.Rules = Request.Cookies["UserRule"];
                ViewBag.RuleName = Request.Cookies["UserRule"];
                ViewBag.MyAccount = Request.Cookies["UserName"];
            }
            catch
            {
            }
            if (!string.IsNullOrEmpty(User.Email) && !string.IsNullOrEmpty(User.UserName) && !string.IsNullOrEmpty(User.PhoneNumber) && User.UserId != 0)
                return true;//Success Login
            else
                return false;//Failed Login
        }
        ////////////////Home Page
        [HttpGet]
        public IActionResult Index()
        {
            if (Check())
                return View();
            else
                return RedirectToAction("Index", "Home");
        }
        //////////////Settings My Account
        public IActionResult Setting()
        {
            if (Check())
            {
                UsersDB User = db.Users.FirstOrDefault(r => r.UserId == int.Parse(Request.Cookies["UserID"]));
                ViewBag.UserSetting = User;
                return View();
            }
            else
                return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Setting(ChPassword User)
        {
            if (Check())
            {
                UsersDB UserCh = db.Users.FirstOrDefault(r => r.UserId == int.Parse(Request.Cookies["UserID"]));
                if (Hash.VerifyPassword(User.OPassword, UserCh.Password))
                {
                    if (User.NPassword == User.RPassword)
                    {
                        UserCh.Password = Hash.HashPassword(User.NPassword);
                        db.SaveChanges();

                        return RedirectToAction("Index", "User");
                    }
                    else
                    {
                        ViewBag.ErrorMessageUser = "The New Password Un same";
                        return View();
                    }
                }
                else
                {
                    ViewBag.ErrorMessageUser = "The Old Password is Error";
                    return View();
                }
            }
            else
                return RedirectToAction("Index", "Home");
        }
        //////////////Delete My Account
        [HttpPost]
        public IActionResult Delete()
        {
            if (Check())
            {
                UsersDB User = db.Users.FirstOrDefault(r => r.UserId == int.Parse(Request.Cookies["UserID"]));
                if (User != null)
                {
                    db.Users.Remove(User);
                    db.SaveChanges();
                }
                HomeController.options = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Delete("UserID");
                Response.Cookies.Delete("UserName");
                Response.Cookies.Delete("UserEmail");
                Response.Cookies.Delete("UserPhone");
                Response.Cookies.Delete("UserRule");
                return RedirectToAction("Index", "Home");
            }
            else
                return RedirectToAction("Index", "Home");

        }






        //////////////Logout From My App
        public IActionResult Logout()
        {
            HomeController.options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-1)
            };
            Response.Cookies.Delete("UserID");
            Response.Cookies.Delete("UserName");
            Response.Cookies.Delete("UserEmail");
            Response.Cookies.Delete("UserPhone");
            Response.Cookies.Delete("UserRule");
            return RedirectToAction("Index", "Home");
        }

    }
}
