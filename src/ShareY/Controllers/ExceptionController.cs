using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareY.Database;

namespace ShareY.Controllers
{
    [Route("exception"), AllowAnonymous]
    public class ExceptionController : ShareYController
    {
        public ExceptionController(ShareYContext dbContext) : base(dbContext)
        {

        }

        [Route("unauthorized"), HttpGet]
#pragma warning disable CS0114
        public IActionResult Unauthorized()
#pragma warning restore CS0114
        {
            return View();
        }
    }
}
