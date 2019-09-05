using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareY.Attributes;
using ShareY.Configurations;
using ShareY.Database;
using ShareY.Extensions;
using ShareY.Interfaces;
using ShareY.Models;
using ShareY.Services;

namespace ShareY.Controllers
{
    [Route("auth")]
    public class AuthController : ShareYController
    {
        private readonly UserController _userController;
        private readonly RoutesConfiguration _routesConfiguration;
        private readonly QuickAuthService _qas;

        public AuthController(ShareYContext dbContext, UserController userController, IRoutesConfigurationProvider routesConfiguration, QuickAuthService qas) : base(dbContext)
        {
            _userController = userController;
            _routesConfiguration = routesConfiguration.GetConfiguration();
            _qas = qas;
        }

        [Route("login"), HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [Route("login/ott"), HttpGet]
        public IActionResult LoginByOtt()
        {
            return View();
        }

        [Route("login/ott"), HttpPost]
        public IActionResult LoginByOttPost(Guid? ottGuid)
        {
            return LoginByOtt(ottGuid);
        }

        [Route("login/ott/{ottGuid}"), HttpGet]
        public IActionResult LoginByOtt(Guid? ottGuid)
        {
            if (!ottGuid.HasValue || ottGuid.Value == default)
            {
                ViewData["ErrorMessage"] = "The given one-time-token was not valid.";
                return View("LoginByOtt");
            }

            try
            {
                var user = _qas.GetAndValidateUserOtt(ottGuid.Value);
                _qas.Invalidate(user);

                HttpContext.Session.Set("userToken", user.Token.Guid);
                return RedirectToAction("Index", controllerName: "Index");
            }
            catch (InvalidOperationException ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                return View("LoginByOtt");
            }
        }

        [Route("login"), HttpPost]
        public async Task<IActionResult> Login(string auth)
        {
            if (string.IsNullOrWhiteSpace(auth))
            {
                ViewData["ErrorMessage"] = "The field is required buddy.";
                return View();
            }

            if (!Guid.TryParse(auth, out var guid))
            {
                var potentialUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email.Equals(auth, StringComparison.OrdinalIgnoreCase));
                if (potentialUser == null)
                {
                    ViewData["ErrorMessage"] = $"Couldn't find any user with email '{auth}'. Please enter a valid e-mail or token to log-in.";
                    return View();
                }

                var ott = _qas.GetOrCreate(potentialUser);
                ViewData["InfoMessage"] = $"A One-Time-Token for the account '{auth}' has been generated. Please check your e-mails and click the magick-url. (debug: {ott.Token})";
                return View();
            }

            var user = await _dbContext.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Token.Guid == guid);
            if (user != null && !user.Disabled && !user.Token.Revoked)
            {
                HttpContext.Session.Set("userToken", guid);
                return RedirectToAction("Index", controllerName: "Index");
            }

            ViewData["ErrorMessage"] = "The provided token was not valid, is revoked, or your account has been disabled.";
            return View();
        }

        [Route("logout"), HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("ShareYSession");
            return RedirectToAction("Index", controllerName: "Index");
        }

        [Route("signup"), HttpGet]
        public IActionResult Signup()
        {
            ViewData["RouteEnabled"] = _routesConfiguration.UserRegisterRoute;

            return View();
        }

        [Route("signup"), HttpPost]
        public async Task<IActionResult> Signup(string email)
        {
            ViewData["RouteEnabled"] = _routesConfiguration.UserRegisterRoute;

            var content = await _userController.CreateUser(new UserCreateModel
            {
                Email = email
            });

            switch (content)
            {
                case OkObjectResult oor:
                    ViewData["Token"] = ((dynamic)oor.Value).Token;
                    break;
                case ObjectResult or:
                    ViewData["RegisterError"] = or.Value != null ? ((string)or.Value) : "";
                    break;
            }

            return View();
        }
    }
}
