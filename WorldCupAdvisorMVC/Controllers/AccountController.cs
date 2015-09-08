using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WorldCupAdvisorMVC.Models;
using Microsoft.AspNet.Identity.Owin;
using WorldCupAdvisorMVC.Model;
using System.Web.Security;

namespace WorldCupAdvisorMVC.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var userDetails = await UserManager.FindAsync(model.UserName, model.Password);
                if (userDetails != null)
                {
                    using (var context = new WorldCupAdvisorContext())
                    {
                        bool getUserStatus = context.UserProfile.Where(a => a.UserName == model.UserName).Select(a => a.Status).FirstOrDefault();

                        if (getUserStatus == true)
                        {
                            await SignInAsync(userDetails, model.RememberMe);
                        }
                    };
                }
            }

            return View(model);
        }

        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Signup(UserProfileModel model)
        {
            try
            {
                var profile = new UserProfile()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.UserName,
                    ContactNumber = model.ContactNumber,
                    Status = true,
                    ActionBy = "User",
                    ActionOn = DateTime.Now,
                };

                using (var context = new WorldCupAdvisorContext())
                {
                    var isExistEmail = context.UserProfile.FirstOrDefault(w => w.Email.Equals(model.Email));
                    var isExistUserName = context.UserProfile.FirstOrDefault(w => w.UserName.Equals(model.UserName));
                    var isExistContactNumber = context.UserProfile.FirstOrDefault(w => w.ContactNumber.Equals(model.ContactNumber));
                    if (isExistEmail == null)
                    {
                        if (isExistUserName == null)
                        {
                            if (isExistContactNumber == null)
                            {
                                try
                                {
                                    context.UserProfile.Add(profile);
                                    context.SaveChanges();
                                    int getUserId = context.UserProfile.OrderByDescending(u => u.Id).Select(a => a.Id).FirstOrDefault();

                                    var user = new WorldCupAdvisorMVC.Models.ApplicationUser()
                                    {
                                        Id = getUserId.ToString(),
                                        UserName = model.UserName,
                                        Email = model.Email,
                                        PasswordHash = model.Password,
                                        PhoneNumber = model.ContactNumber
                                    };

                                    var result = await UserManager.CreateAsync(user, model.Password);
                                    //var result = await UserManager.CreateAsync(user, newPassword);

                                    if (result.Succeeded)
                                    {
                                        result = UserManager.AddToRole(user.Id, "User");
                                        await SignInAsync(user, isPersistent: false);
                                    }
                                    else
                                    {
                                        context.UserProfile.Remove(profile);
                                        context.SaveChanges();
                                        //AddErrors(result);
                                        return RedirectToAction("Signup", "Account");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                    return RedirectToAction("Signup", "Account");
                                }
                            }
                            else
                            {
                                return RedirectToAction("Signup", "Account");
                            }
                        }
                        else
                        {
                            return RedirectToAction("Signup", "Account");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Signup", "Account");
                    }
                };
            }
            catch (Exception ex)
            {
                return RedirectToAction("Signup", "Account");
            }

            return View(model);
        }

        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Sign in with Identity.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isPersistent"></param>
        /// <returns></returns>
        private async Task SignInAsync(WorldCupAdvisorMVC.Models.ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            var identity = await UserManager.CreateIdentityAsync(
               user, DefaultAuthenticationTypes.ApplicationCookie);

            AuthenticationManager.SignIn(
               new AuthenticationProperties()
               {
                   IsPersistent = isPersistent
               }, identity);
        }

        [HttpPost]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new WorldCupAdvisorMVC.Controllers.AccountController.ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();

            if (loginInfo == null)
            {
                return RedirectToAction("Index");
            }

            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                     int getUserId = 0;
                    var user = await UserManager.FindByEmailAsync(loginInfo.Email);

                    if (user != null)
                    {
                        //int getUserId =Convert.ToInt32(User.Identity.GetUserId());
                        getUserId = Convert.ToInt32(user.Id);
                    }

                    if (getUserId != 0)
                    {
                        using (var context = new WorldCupAdvisorContext())
                        {
                            var isAuthenticateUser = UserManager.IsInRole(user.Id, "User");
                            if (isAuthenticateUser == true)
                            {
                                bool getUserStatus = context.UserProfile.Where(a => a.Id == getUserId).Select(a => a.Status).FirstOrDefault();
                        

                                if (getUserStatus == true)
                                {
                                    return RedirectToAction("Index", "Dashboard");
                                }
                            }                           
                        };
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account");
                    }
                    break;

                case SignInStatus.Failure:
                default:
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }

            return View();
        }

        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {

                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();

                if (info == null)
                {
                    return View("Login");
                }

                using (var context = new WorldCupAdvisorContext())
                {
                    var profile = new UserProfile()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        UserName = model.UserName,
                        Email = info.Email,
                        ContactNumber = model.ContactNumber,
                        Status = true,
                        ActionBy = "Google+",
                        ActionOn = DateTime.Now,
                    };

                    context.UserProfile.Add(profile);
                    context.SaveChanges();
                    int getUserId = context.UserProfile.OrderByDescending(u => u.Id).Select(a => a.Id).FirstOrDefault();

                    var user = new WorldCupAdvisorMVC.Models.ApplicationUser()
                    {
                        Id = getUserId.ToString(),
                        UserName = model.UserName,
                        Email = info.Email,
                        PhoneNumber = model.ContactNumber
                    };

                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        result = await UserManager.AddLoginAsync(user.Id, info.Login);
                        result = UserManager.AddToRole(user.Id, "User");

                        if (result.Succeeded)
                        {
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("Index", "Dashboard");
                        }
                    }
                    else
                    {
                        context.UserProfile.Remove(profile);
                        context.SaveChanges();
                    }

                    //AddErrors(result);
                };

            return View(model);
        }

        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        private const string XsrfKey = "XsrfId";
    }
}