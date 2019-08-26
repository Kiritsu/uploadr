using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShareY.Controllers
{
    [Route("admin"), AllowAnonymous]
    public class AdminController : ShareYController
    {
        [Route(""), HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
