using CinemaProject.Models;
using Microsoft.AspNetCore.Mvc;

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
                ViewBag.MyAccount = Request.Cookies["UserName"];
            }
            catch
            {
            }
            if (!string.IsNullOrEmpty(User.Email) && !string.IsNullOrEmpty(User.UserName) && !string.IsNullOrEmpty(User.PhoneNumber) && User.UserId != 0)
                return "1";//Success Login
            else
                return "0";//Failed Login
        }
        ////////////////Home Page
        [HttpGet]
        public IActionResult Index()
        {
            if (Check() == "1")
                return View();
            else
                return RedirectToAction("Index", "Home");
        }
        //////////////Settings My Account
        public IActionResult Setting()
        {
            if (Check() == "1")
            {
                UsersDB User = db.Users.FirstOrDefault(r => r.UserId == int.Parse(Request.Cookies["UserID"]));
                ViewBag.UserSetting = User;
                return View();
            }
            else
                return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public IActionResult Setting(UsersDB User)
        {

            return RedirectToAction("Index", "User");

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
