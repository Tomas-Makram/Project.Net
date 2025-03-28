using CinemaProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FirebaseAdmin.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CinemaProject.Controllers
{
    public class HomeController : Controller
    {
        //Database
        private readonly CinemaDB db;
        //Firebase
        private readonly FirebaseAuthClient _authClient;
        private readonly string _firebaseApiKey;

        //ReCaptcha
        private readonly HttpClient _httpClient;
        private readonly string _recaptchaSecretKey;

        public HomeController(CinemaDB _db, IConfiguration configg)
        {
            this.db = _db;
            _firebaseApiKey = configg["Firebase:ApiKey"];
            _recaptchaSecretKey = configg["ReCapchaSettings:SecretKey"];
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("Firebase.json")
                });
            }

            var config = new FirebaseAuthConfig
            {
                ApiKey = configg["Firebase:ApiKey"],
                AuthDomain = configg["Firebase:AuthDomain"],
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                }
            };
            _authClient = new FirebaseAuthClient(config);
            _httpClient = new HttpClient();
        }

        //To Save Login 
        public static CookieOptions options;
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
                User.Rules = Request.Cookies["UserRule"];

            }
            catch
            {
            }
            if (!string.IsNullOrEmpty(User.Email) && !string.IsNullOrEmpty(User.UserName) && !string.IsNullOrEmpty(User.PhoneNumber) && User.UserId != 0)
                return true;//Success Login
            else
                return false;//Failed Login
        }

        ////////////////Authantcation Emails////////////////////////

        //Check if Email in database or not
        private async Task<bool> CheckCreateNewAccount(string email)
        {
            try
            {
                var auth = FirebaseAuth.DefaultInstance;
                var user = await auth.GetUserByEmailAsync(email);

                if (user != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //Create New Account in database with default EmailVerified false
        private async Task<bool> CreateNewAccount(string email, string password, bool EmailVerified = false)
        {
            try
            {
                var userArgs = new UserRecordArgs
                {
                    Email = email,
                    Password = password,
                    EmailVerified = EmailVerified,
                    Disabled = false
                };

                UserRecord userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
                return true;
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
       
        //Get ID Token To Can Send Emails 
        private async Task<string> GetIdToken(string email, string password)
        {
            try
            {
                var requestBody = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_firebaseApiKey}",
                    requestBody
                );

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

                if (response.IsSuccessStatusCode)
                {
                    string idToken = responseDict["idToken"].ToString();
                    string uid = responseDict["localId"].ToString();
                    string userEmail = responseDict["email"].ToString();
                    return idToken;
                }

                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        //Send Email Verification to Email Client by ID token
        private async Task<bool> SendVerification(string idToken)
        {
            try
            {
                var requestBody = new
                {
                    requestType = "VERIFY_EMAIL",
                    idToken = idToken
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_firebaseApiKey}",
                    requestBody
                );

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //Check if Account is Verification or not
        private async Task<bool> CheckVerification(string email, string password)
        {
            try
            {
                var userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
                if (userRecord.EmailVerified)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        //Delete Account from Database
        public async Task<bool> DeleteAccount(string email)
        {
            try
            {
                var userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);

                if (userRecord != null)
                {
                    await FirebaseAuth.DefaultInstance.DeleteUserAsync(userRecord.Uid);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Firebase.Auth.FirebaseAuthException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //When i Create new account in my System use this function to Check my Email is realy or not
        private async Task<bool> CheckEmail(string email, string password)
        {
            if (!(await CheckCreateNewAccount(email)))
            {
                if (await CreateNewAccount(email, password))
                {
                    if (await SendVerification(await GetIdToken(email, password)))
                    {
                        await CheckVerification(email, password);
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("This is the fake Email");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Can not Use this Email");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("The Email is Found in database please check your mail");
                return false;
            }
        }

        //Check if Recaptcha is true or not (I am not robot)
        private async Task<bool> ValidateRecaptcha(string recaptchaResponse)
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "secret", _recaptchaSecretKey },
                    { "response", recaptchaResponse }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(json))
                {
                    return false;
                }

                var result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                return result != null && result.TryGetValue("success", out JsonElement successElement) && successElement.GetBoolean();

            }
        }

        //Send Email To Reset Password my Account
        private async Task<bool> SendPasswordResetEmail(string email)
        {
            try
            {
                var requestBody = new
                {
                    requestType = "PASSWORD_RESET",
                    email = email
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_firebaseApiKey}",
                    requestBody
                );

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //Check if Success SignIn or not
        private async Task<bool> SignInWithEmailAndPassword(string email, string password)
        {
            try
            {
                var requestBody = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_firebaseApiKey}",
                    requestBody
                );

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        ////////////////IndexPage////////////////////////
        /////Public Page
        public IActionResult Index()
        {
            if (Check())//If User Login in System
                return RedirectToAction("Index", "User");//Go to Home User
            else
                return View();//If not go to Index Home
        }

        ///////////////Login Page/////////////////////
        [HttpGet]//Public Page
        public IActionResult Login()
        {
            if (!Check())//Check if login or not
                return View();//If Not Go to Login Page
            else
                return RedirectToAction("Index", "User");//If Login Go To Home Users Page
        }

        ///////////////Login Page
        [HttpPost]//Private Page
        public async Task<IActionResult> Login(Login Login, string recaptchaResponse)
        {
            var login = db.Users.ToArray();//Get All Accounts in System

            //Check [i am not robot]
            if (!await ValidateRecaptcha(recaptchaResponse))
            {
                ViewBag.ErrorMessageUser = "Please check you not a Robot.";
                return View();
            }

            //Check if User want to reset password or not
            if (await CheckCreateNewAccount(Login.UName))//Check if account in database or not
            {
                if (await CheckVerification(Login.UName, Login.Password))//Check if account is Verification or not
                {
                    if (await SignInWithEmailAndPassword(Login.UName, Login.Password))//Check if User and password is true
                    {
                        var Ch = login.FirstOrDefault(u => u.Email == Login.UName) != null ? login.FirstOrDefault(u => u.Email == Login.UName) : null;
                        if (Ch != null)
                        {
                            Ch.Password = Hash.HashPassword(Login.Password);//Change Password Account System
                            await db.SaveChangesAsync();//Save Change
                            await DeleteAccount(Login.UName);//Delete Account in database
                        }
                        else
                        {
                            ViewBag.ErrorMessageUser = "Please return to Signup and rewrite a same data.";
                            return View();
                        }
                    }
                }
            }

            //Check if my account in the system 
            foreach (var user in login)
            {
                if ((user.UserName == Login.UName || user.PhoneNumber == Login.UName || user.Email == Login.UName) && Hash.VerifyPassword(Login.Password, user.Password))
                {
                    //Start Cookies 7 Days
                    options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7)
                    };
                    Response.Cookies.Append("UserID", user.UserId.ToString(), options);
                    Response.Cookies.Append("UserName", user.UserName, options);
                    Response.Cookies.Append("UserEmail", user.Email, options);
                    Response.Cookies.Append("UserPhone", user.PhoneNumber, options);
                    Response.Cookies.Append("UserRule", user.Rules, options);
                    //Go in Home Users
                    return RedirectToAction("Index", "User");
                }
            }
            //The account not in my system
            ViewBag.ErrorMessageUser = "The UserName or Password not found";
            return View();
        }

        ///////////////Sign-Up Page/////////////////////
        [HttpGet]//Public Page
        public IActionResult Signup()
        {
            if (!Check())//Check if login or not
                return View();//If Not Go to Signup Page
            else
                return RedirectToAction("Index", "User");//If Login Go To Home Users Page
        }
        ///////////////Sign-Up Page
        [HttpPost]//Private Page
        public async Task<IActionResult> Signup(UsersDB New, string recaptchaResponse)
        {
            //Check the Captcha [i am not robot]
            if (!await ValidateRecaptcha(recaptchaResponse))
            {
                ViewBag.ErrorMessageUser = "Please check you not a Robot.";
                return View();
            }

            //Get all account in the system 
            var User = db.Users.ToList();
            if (New.PhoneNumber[0] == '0' && New.PhoneNumber[1] == '1' && New.PhoneNumber.Length == 11)//Check the phone number
            {
                foreach (var user in User)
                {
                    if (user.Email == New.Email || user.PhoneNumber == New.PhoneNumber)//Check the Email and phone number not in the system
                    {
                        ViewBag.ErrorMessageUser = "This Email or Phone Number was found";
                        return View();
                    }
                }

                //Check Send Verification in Email User
                await CheckEmail(New.Email, New.Password);
                //Check if Email is Vitrificated or not
                if (await CheckVerification(New.Email, New.Password))
                {
                    //Create new object to add account 
                    var newU = new UsersDB { UserName = New.UserName, PhoneNumber = New.PhoneNumber, Email = New.Email, Password = Hash.HashPassword(New.Password), Rules = Hash.HashPassword("User") };
                    db.Users.Add(newU);
                    db.SaveChanges();
                    await DeleteAccount(New.Email);//Delete Account in database
                    return RedirectToAction("Login");//Go To login Page
                }
                ViewBag.ErrorMessageUser = "Check Your Mail To Validation and sign-up again with same data";
                return View();
            }
            else
            {
                ViewBag.ErrorMessageUser = "The Phone Number must start with 01 and length 11 number";
                return View();
            }
        }
        ///////////////Reset Password Page/////////////////////
        [HttpGet]//Public Page
        public IActionResult ResetPassword()
        {
            if (!Check())//If User not Login
                return View();//Go to Reset Password page
            else
                return RedirectToAction("Index", "User");//Go to Home User Page
        }
        ////////////////////Reset Password Page
        [HttpPost]//Private Page
        public async Task<IActionResult> ResetPassword(string Email, string recaptchaResponse)
        {
            //Check the Captcha [i am not robot]
            if (!await ValidateRecaptcha(recaptchaResponse))
            {
                ViewBag.MessageUser = "Please check you not a Robot.";
                return View();
            }

            //Get All Account in System
            var Users = db.Users.ToList();
            //Get Account by Email
            var Ch = Users.FirstOrDefault(u => u.Email == Email) != null ? Users.FirstOrDefault(u => u.Email == Email) : null;
            
            //Check if Account is in system 
            if (Ch != null)
            {
                //Check if Account in database
                if (!(await CheckCreateNewAccount(Email)))
                {
                    //Create Account in database With true Verification
                    if (await CreateNewAccount(Email, "12345678qwertyui!@#@", true))
                    {
                        //Send Email Reset Password
                        await SendPasswordResetEmail(Email);
                    }
                }
                ViewBag.MessageUser = "Check Your Mail and Go to login";
            }
            else
            {
                ViewBag.MessageUser = "This Email not found";
            }
            return View();
        }

        //////////////////////////Error Page/////////////////////
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
//CheckCreateNewAccount();
//CreateNewAccount();
//SendPasswordResetEmail();
//SignInWithEmailAndPassword();
//DeleteAccount();


//var authResult = await _authClient.SignInWithEmailAndPasswordAsync(email, password);
//var user = authResult.User;