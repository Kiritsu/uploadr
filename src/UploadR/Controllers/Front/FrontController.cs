using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UploadR.Configurations;
using UploadR.Database.Enums;
using UploadR.Interfaces;

namespace UploadR.Controllers.Front
{
    [Route("api/[controller]"), ApiController]
    public class FrontController : Controller
    {
        private readonly RoutesConfiguration _routes;

        public FrontController(IRoutesConfigurationProvider routesConfig)
        {
            _routes = routesConfig.GetConfiguration();
        }

        [HttpGet, Route("@me")]
        public IActionResult GetNavMenuInfosAsync()
        {
            return Json(new
            {
                CanSignup = _routes.UserRegisterRoute,
                IsAuthenticated = User?.Identity?.IsAuthenticated,
                IsAdmin = User?.IsInRole(AccountType.Admin.ToString())
            });
        }
    }
}
