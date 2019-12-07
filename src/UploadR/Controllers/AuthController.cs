using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UploadR.Configurations;
using UploadR.Database;
using UploadR.Extensions;
using UploadR.Interfaces;
using UploadR.Models;
using UploadR.Services;

namespace UploadR.Controllers
{
    [Route("auth")]
    public class AuthController : UploadRController
    {
        private readonly UserController _userController;
        private readonly RoutesConfiguration _routesConfiguration;
        private readonly OneTimeTokenConfiguration _ottConfiguration;
        private readonly QuickAuthService _qas;
        private readonly EmailService _emails;
        private readonly ILogger<AuthController> _logger;
        private readonly Random _rng;

        public AuthController(UploadRContext dbContext, UserController userController,
            IRoutesConfigurationProvider routesConfiguration, QuickAuthService qas,
            IOneTimeTokenConfigurationProvider ottConfiguration, EmailService emails,
            ILogger<AuthController> logger, Random rng) : base(dbContext)
        {
            _userController = userController;
            _routesConfiguration = routesConfiguration.GetConfiguration();
            _ottConfiguration = ottConfiguration.GetConfiguration();
            _qas = qas;
            _emails = emails;
            _logger = logger;
            _rng = rng;
        }

        [Route("login"), HttpGet]
        public IActionResult Login()
        {
            ViewData["EnableOttButton"] = _ottConfiguration.Enabled;

            return View();
        }

        [Route("login/ott"), HttpGet]
        public IActionResult LoginByOtt()
        {
            ViewData["OttEnabled"] = _ottConfiguration.Enabled;

            return View();
        }

        [Route("login/ott"), HttpPost]
        public IActionResult LoginByOttPost(Guid? ottGuid)
        {
            ViewData["OttEnabled"] = _ottConfiguration.Enabled;

            return LoginByOtt(ottGuid);
        }

        [Route("login/ott/{ottGuid}"), HttpGet]
        public IActionResult LoginByOtt(Guid? ottGuid)
        {
            var hashIp = HttpContext.Connection.RemoteIpAddress.GetHashCode();
            if (!_qas.IncrementAndValidateRateLimits(hashIp))
            {
                ViewData["ErrorMessage"] = $"You are being rate limited. Retry in {_qas.GetRemainingTimeout(hashIp)} minutes.";
            }

            ViewData["OttEnabled"] = _ottConfiguration.Enabled;

            if (!_ottConfiguration.Enabled)
            {
                return View("LoginByOtt");
            }

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
            var hashIp = HttpContext.Connection.RemoteIpAddress.GetHashCode();
            if (!_qas.IncrementAndValidateRateLimits(hashIp))
            {
                ViewData["ErrorMessage"] = $"You are being rate limited. Retry in {_qas.GetRemainingTimeout(hashIp)} minutes.";
            }

            ViewData["EnableOttButton"] = _ottConfiguration.Enabled;

            if (string.IsNullOrWhiteSpace(auth))
            {
                ViewData["ErrorMessage"] = "The field is required.";
                return View();
            }

            if (!Guid.TryParse(auth, out var guid))
            {
                if (!auth.IsValidEmail())
                {
                    ViewData["ErrorMessage"] = "The field has to contain a valid e-mail or token.";
                    return View();
                }

                if (!_ottConfiguration.Enabled)
                {
                    ViewData["ErrorMessage"] = "The provided token was not valid, is revoked, or your account has been disabled.";
                    return View();
                }

                var potentialUser = await _dbContext.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Email.Equals(auth, StringComparison.OrdinalIgnoreCase));

                OneTimeToken ott = null;
                if (potentialUser != null)
                {
                    if (potentialUser.Disabled || potentialUser.Token.Revoked)
                    {
                        ViewData["ErrorMessage"] = "This account is either disabled or your token has not been renewed.";
                        return View();
                    }

                    try
                    {
                        ott = _qas.GetOrCreate(potentialUser);
                    }
                    catch (InvalidOperationException ex)
                    {
                        ViewData["ErrorMessage"] = ex.Message;
                        return View();
                    }
                }

                try
                {
                    await _emails.SendMagickUrlAsync(ott, $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Host}");
                }
                catch (Exception e)
                {
                    var debugId = $"#{_rng.Next(100, 9999)}-{DateTimeOffset.Now.Ticks}";
                    _logger.LogError($"An error happened with email service (debug id {debugId}): {e.Message}. Stack trace: {e.StackTrace}");

                    ViewData["ErrorMessage"] = $"Something really bad happened. Please report this issue with the following id: {debugId}";
                    return View();
                }

                ViewData["InfoMessage"] = $"A One-Time-Token for the account '{auth}' has been generated. If the account exists, please check your e-mails and click the magick-url.";

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
            Response.Cookies.Delete("PotatoSession");
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
                    var token = ((dynamic)oor.Value).Token;
                    ViewData["Token"] = token;
                    try
                    {
                        await _emails.SendSignupSuccessAsync(token, $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Host}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "SendSignupSuccess error");
                        ViewData["RegisterError"] = "Internal error: couldn't send a verification email.";
                    }
                    break;
                case ObjectResult or:
                    ViewData["RegisterError"] = or.Value != null ? (string)or.Value : "";
                    break;
            }

            return View();
        }
    }
}
