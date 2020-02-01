using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UploadR.Configurations;
using UploadR.Database;
using UploadR.Enum;
using UploadR.Extensions;
using UploadR.Interfaces;
using UploadR.Models;
using UploadR.Services;

namespace UploadR.Controllers
{
    [Route("auth")]
    public class AuthController : UploadRController
    {
        private readonly UploadRContext _dbContext;
        private readonly RoutesConfiguration _routesConfiguration;
        private readonly OneTimeTokenConfiguration _ottConfiguration;
        private readonly QuickAuthService _qas;
        private readonly EmailService _emails;
        private readonly ILogger<AuthController> _logger;
        private readonly Random _rng;
        private readonly UserService _us;

        public AuthController(UploadRContext dbContext,
            IRoutesConfigurationProvider routesConfiguration, QuickAuthService qas,
            IOneTimeTokenConfigurationProvider ottConfiguration, EmailService emails,
            ILogger<AuthController> logger, Random rng, UserService us)
        {
            _dbContext = dbContext;
            _routesConfiguration = routesConfiguration.GetConfiguration();
            _ottConfiguration = ottConfiguration.GetConfiguration();
            _qas = qas;
            _emails = emails;
            _logger = logger;
            _rng = rng;
            _us = us;
        }

        [Route("login/ott"), HttpGet]
        public IActionResult LoginByOttGet()
        {
            return View();
        }

        [Route("login/ott"), HttpPost]
        public Task<IActionResult> LoginByOttPost(Guid? ott)
        {
            return LoginByOtt(ott);
        }

        [Route("login/ott/{ottGuid}"), HttpGet]
        public async Task<IActionResult> LoginByOtt(Guid? ottGuid)
        {
            var hashIp = HttpContext.Connection.RemoteIpAddress.GetHashCode();
            if (!_qas.IncrementAndValidateRateLimits(hashIp))
            {
                ViewData["ErrorMessage"] = $"You are being rate limited. Retry in {_qas.GetRemainingTimeout(hashIp)} minutes.";
                return View("LoginByOtt");
            }

            if (!ottGuid.HasValue || ottGuid.Value == default)
            {
                ViewData["ErrorMessage"] = "The given one-time-token was not valid.";
                return View("LoginByOtt");
            }

            try
            {
                var user = await _qas.GetAndValidateUserOttAsync(ottGuid.Value);
                _qas.Invalidate(user);

                if (string.IsNullOrWhiteSpace(user.Token))
                {
                    ViewData["ErrorMessage"] = "Your account doesn't have any access token set.";
                    return View("LoginByOtt");
                }

                HttpContext.Session.Set("UserToken", user.Token);
                return RedirectToAction("Index", controllerName: "Index");
            }
            catch (InvalidOperationException ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                return View("LoginByOtt");
            }
        }

        [Route("login"), HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [Route("login"), HttpPost]
        public async Task<IActionResult> Login(string email)
        {
            var hashIp = HttpContext.Connection.RemoteIpAddress.GetHashCode();
            if (!_qas.IncrementAndValidateRateLimits(hashIp))
            {
                ViewData["ErrorMessage"] = $"You are being rate limited. " +
                    $"Retry in {_qas.GetRemainingTimeout(hashIp)} minutes.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ViewData["ErrorMessage"] = "An email is required.";
                return View();
            }

            if (!email.IsValidEmail())
            {
                ViewData["ErrorMessage"] = "A valid email is required.";
                return View();
            }

            var potentialUser = await _dbContext.Users.FirstOrDefaultAsync(
                x => x.Email.ToLower() == email.ToLower());

            if (potentialUser != null)
            {
                if (potentialUser.Disabled)
                {
                    ViewData["ErrorMessage"] = "This account is disabled.";
                    return View();
                }

                OneTimeToken ott = null;
                try
                {
                    ott = _qas.GetOrCreate(potentialUser);
                }
                catch (InvalidOperationException ex)
                {
                    ViewData["ErrorMessage"] = ex.Message;
                    return View();
                }

                if (ott != null)
                {
                    try
                    {
                        await _emails.SendMagickUrlAsync(ott,
                            $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Host}");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Unable the send the magick url email.");
                        ViewData["ErrorMessage"] = "Could not send the email. Please try again later.";
                        return View();
                    }
                }
            }

            ViewData["InfoMessage"] = $"A One-Time-Token for the account '{email}' has been generated. " +
                $"If the account exists, please check your e-mails and click the magick-url.";

            return View();
        }

        [Route("logout"), HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("UploadRSession");
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

            var model = new UserCreateModel
            {
                Email = email
            };

            var result = await _us.CreateAccountAsync(model);

            if (result.IsSuccess)
            {
                ViewData["Token"] = result.Value.Token;

                try
                {
                    await _emails.SendSignupSuccessAsync(result.Value, result.Value.Token,
                        $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Host}");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to send email.");
                    ViewData["RegisterError"] = "Internal error: couldn't send the email.";
                }
            }
            else
            {
                if (result.Code == ResultErrorType.Found)
                {
                    ViewData["RegisterError"] = "A user with that email already exists.";
                } 
                else if (result.Code == ResultErrorType.EmailNotProvided)
                {
                    ViewData["RegisterError"] = "Please provide a valid email.";
                }
            }

            return View();
        }
    }
}
