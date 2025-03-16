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
            if (!string.IsNullOrEmpty(User.Email) && !string.IsNullOrEmpty(User.UserName) && !string.IsNullOrEmpty(User.PhoneNumber) && User.UserId != 0 && Request.Cookies["RulesName"] == "Movies")
                return "1";
            else
                return "0";
        }
        /// //////////////////////
        [HttpGet]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult New(UsersDB usersDB)
        {
            var Users = db.Users.ToArray();
            foreach (var user in Users)
            {
                if (user.UserName == usersDB.UserName)
                    return View();
            }
            db.Users.Add(usersDB);
            db.SaveChanges();
            return View();
        }
        ///////////////////////////Admin Setting
        [HttpGet]
        public IActionResult Admin()
        {
            return View();
        }

        ///////////////////////////Show All Rules
        [HttpGet]
        public IActionResult Rules()
        {
            var Rules = db.Rules.ToList();
            ViewBag.Rules = Rules;
            return View();
        }
        ///////////////////////////Delete Rules
        [HttpPost]
        public IActionResult Rules(int ID)
        {
            var rule = db.Rules.FirstOrDefault(r => r.RulesID == ID);

            if (rule != null)
            {
                db.Rules.Remove(rule);
                db.SaveChanges();
            }
            return Redirect("Rules");
        }
        ///////////////////////////New Rules
        [HttpPost]
        public IActionResult NewRules(string newRule)
        {
            if (!string.IsNullOrWhiteSpace(newRule))
            {
                var existingRule = db.Rules.FirstOrDefault(r => r.RulesName.ToLower() == newRule.ToLower());
                if (existingRule == null)
                {
                    var newR = new RulesDB { RulesName = newRule };
                    db.Rules.Add(newR);
                    db.SaveChanges();
                }
            }
            return Redirect("Rules");
        }

        ///////////////////////////Admin Setting (Movies) Check if User Admin 
        public IActionResult Movies()
        {
            string ViewPage = Check();
            if (ViewPage == "1")
            {
                if (Request.Cookies["RulesName"] == "Movies")
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
