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
    [Route("login"), AllowAnonymous]
    public class LoginController : ShareYController
    {
        private readonly ShareYContext _dbContext;
        private readonly UserController _userController;
        private readonly RoutesConfiguration _routesConfiguration;

        public LoginController(ShareYContext dbContext, UserController userController, IRoutesConfigurationProvider routesConfiguration)
        {
            _dbContext = dbContext;
            _userController = userController;
            _routesConfiguration = routesConfiguration.GetConfiguration();
        }

        [Route(""), HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Route(""), HttpPost]
        public async Task<IActionResult> Index(Guid? userToken)
        {
            if (userToken.HasValue)
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Token.Guid == userToken);
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

        [Route("new"), HttpGet]
        public IActionResult Create()
        {
            ViewData["RouteEnabled"] = _routesConfiguration.UserRegisterRoute;

            return View();
        }

        [Route("new"), HttpPost]
        public async Task<IActionResult> Create(string email)
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
