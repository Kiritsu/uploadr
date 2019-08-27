using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareY.Configurations;
using ShareY.Database;
using ShareY.Extensions;
using ShareY.Interfaces;
using ShareY.Models;

namespace ShareY.Controllers
{
    [Route("auth"), AllowAnonymous]
    public class AuthController : ShareYController
    {
        private readonly UserController _userController;
        private readonly RoutesConfiguration _routesConfiguration;

        public AuthController(ShareYContext dbContext, UserController userController, IRoutesConfigurationProvider routesConfiguration) : base(dbContext)
        {
            _userController = userController;
            _routesConfiguration = routesConfiguration.GetConfiguration();
        }

        [Route("login"), HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [Route("login"), HttpPost]
        public async Task<IActionResult> Login(Guid? userToken)
        {
            if (userToken.HasValue)
            {
                var user = await _dbContext.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Token.Guid == userToken);
                if (user != null)
                {
                    HttpContext.Session.Set("userToken", userToken);
                    TempData["InfoMessage"] = "You have successfully logged in.";
                    return Redirect("/");
                }
            }

            ViewData["ErrorMessage"] = "The provided token was not valid.";
            return View();
        }

        [Route("logout"), HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("ShareYSession");
            TempData["InfoMessage"] = "You have successfully logged out.";
            return Redirect("/");
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
                    ViewData["RegisterError"] = or.Value != null ? ((string) or.Value) : "";
                    break;
            }

            return View();
        }
    }
}
