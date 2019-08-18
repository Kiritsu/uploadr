using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareY.Database;
using ShareY.Extensions;

namespace ShareY.Controllers
{
    [Route("login"), AllowAnonymous]
    public class LoginController : ShareYController
    {
        private readonly ShareYContext _dbContext;

        public LoginController(ShareYContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route(""), HttpGet]
        public IActionResult Index()
        {
            return View(Model);
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
                    return Redirect("/");
                }
            }

            ViewData["ErrorMessage"] = "The provided token was not valid.";
            return View(Model);
        }
    }
}
