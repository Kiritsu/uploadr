using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShareY.Controllers
{
    [Route(""), ApiController, AllowAnonymous]
    public class IndexController : Controller
    {
        [Route(""), HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
