using CinemaProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Net;

namespace CinemaProject.Controllers
{
    public class AdminController : Controller
    {
        //Database
        private readonly CinemaDB db;
        public AdminController(CinemaDB db)
        {
            this.db = db;
        }

        ////////////////////Functions
        private static string Filter = "All"; //Filter Users List
        private static string ErrorMassage = ""; // Error Massage on Create New Admin

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
                return true;
            else
                return false;
        }
        /// ////////////////////// <summary>
        public IActionResult Index()
        {
            if (Check())
                return RedirectToAction("Index", "User");

            else
                return RedirectToAction("Index", "Home");

        }

        ///////////////////////////Admin Setting (Admin) Check if User Admin 
        /////////////////////////Home Page Admin Settings
        [HttpGet]
        public IActionResult Admin()
        {
            if (Check())
            {
                if (Hash.VerifyPassword("Admin", Request.Cookies["UserRule"]))
                {
                    var users = db.Users.ToList();
                    ViewBag.MessageError = ErrorMassage;
                    ErrorMassage = "";
                    if (Filter != "All")
                    {
                        ViewBag.Users = users.Where(u => u.Rules == (Hash.VerifyPassword(Filter, u.Rules) ? u.Rules : "0")).ToList();
                    }
                    else
                        ViewBag.Users = users;
                    return View("Admin");
                }
                else
                    return RedirectToAction("Index", "User");
            }
            else
                return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Admin(UsersDB Users)
        {
            return View();
        }
        /////////////////////////Admin Filter Admins-Users
        [HttpPost]
        public IActionResult AdminFilter()
        {
            Filter = Request.Form["btnradio"];
            return RedirectToAction("Admin", "Admin");
        }
        ///////////////////////////Admin Setting Add Account
        [HttpPost]
        public IActionResult AddNewAdmin(UsersDB New)
        {
            var Users = db.Users.ToArray();
            if (Users.Any(u => u.Email == New.Email) || Users.Any(u => u.PhoneNumber == New.PhoneNumber))
            {
                ErrorMassage = "The Email or Phone Number Already Used";
                return RedirectToAction("Admin", "Admin");
            }
            var newU = new UsersDB { UserName = New.UserName, Email = New.Email, PhoneNumber = New.PhoneNumber, Rules = Hash.HashPassword(New.Rules), Password = Hash.HashPassword(New.Email) };
            db.Users.Add(newU);
            db.SaveChanges();
            return RedirectToAction("Admin", "Admin");
        }
        ///////////////////////////Admin Setting Delete Account
        [HttpPost]
        public IActionResult DeleteAccount(int ID)
        {
            var User = db.Users.FirstOrDefault(r => r.UserId == ID);
            if (User != null)
            {
                db.Users.Remove(User);
                db.SaveChanges();
            }
            return RedirectToAction("Admin");
        }

        ///////////////////////////Admin Setting (Movies) Check if User Admin 
        public IActionResult Movies()
        {
            if (Check())
            {
                if (Hash.VerifyPassword("Admin", Request.Cookies["UserRule"]))
                {
                    ViewBag.Movies = db.Movies.ToList();
                }
                return View("MoviesSetting");
            }
            else
                return RedirectToAction("Index", "Home");
        }
        ///////////////////////////Home Page Movie Settings
        [HttpPost]
        public IActionResult MoviesSetting()
        {
            return View();
        }
        ///////////////////////////Add New Movie
        [HttpPost]
        public IActionResult AddMovie(MoviesDB Movies)
        {
            if (!string.IsNullOrWhiteSpace(Movies.MovieName))
            {
                var newM = new MoviesDB { MovieName = Movies.MovieName, MovieDescription = Movies.MovieDescription, MovieGenre = Movies.MovieGenre, MovieDuration = Movies.MovieDuration, MoviePosterURL = Movies.MoviePosterURL, MovieLaguage = Movies.MovieLaguage, MovieSubTitle = Movies.MovieSubTitle };
                db.Movies.Add(newM);
                db.SaveChanges();
            }
            return Redirect(Url.Action("Movies", "Admin") + "#Show");
        }
        ///////////////////////////Update Movie
        [HttpPost]
        public IActionResult UpdateMovie(int ID)
        {
            Check();
            MoviesDB Movies = db.Movies.FirstOrDefault(r => r.MovieID == ID);
            if (Movies == null)
            {
                return NotFound();
            }
            TempData["UpdateMovie"] = Movies;
            return View("MoviesSetting");
        }
        [HttpPost]
        public IActionResult UpdateMovieSetting(MoviesDB UpdateMovie)
        {
            MoviesDB Movies = db.Movies.FirstOrDefault(r => r.MovieID == UpdateMovie.MovieID);
            if (Movies == null)
            {
                return NotFound();
            }
            Movies.MovieName = UpdateMovie.MovieName;
            Movies.MovieGenre = UpdateMovie.MovieGenre;
            Movies.MovieDuration = UpdateMovie.MovieDuration;
            Movies.MovieDescription = UpdateMovie.MovieDescription;
            Movies.MovieLaguage = UpdateMovie.MovieLaguage;
            Movies.MovieSubTitle = UpdateMovie.MovieSubTitle;
            Movies.MoviePosterURL = UpdateMovie.MoviePosterURL;
            db.SaveChanges();
            return Redirect(Url.Action("Movies", "Admin") + "#Show");
        }

        ///////////////////////////Delete Movie
        [HttpPost]
        public IActionResult DeleteMovie(int ID)
        {
            var Movies = db.Movies.FirstOrDefault(r => r.MovieID == ID);
            if (Movies != null)
            {
                db.Movies.Remove(Movies);
                db.SaveChanges();
            }
            return RedirectToAction("Movies");
        }

    }
}

/// ////////////////////// <summary>
//[HttpGet]
//public IActionResult New()
//{
//    return View();
//}

//[HttpPost]
//[ValidateAntiForgeryToken]
//public IActionResult New(UsersDB usersDB)
//{
//    var Users = db.Users.ToArray();
//    foreach (var user in Users)
//    {
//        if (user.UserName == usersDB.UserName)
//            return View();
//    }
//    db.Users.Add(usersDB);
//    db.SaveChanges();
//    return View();
//}