using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareY.Database;
using ShareY.Database.Enums;

namespace ShareY.Controllers
{
    [Route("admin"), AllowAnonymous]
    public class AdminController : ShareYController
    {
        public AdminController(ShareYContext dbContext) : base(dbContext)
        {
        }

        [Route(""), HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated || !IsAdmin)
            {
                return LocalRedirect("/exception/unauthorized");
            }

            return View();
        }
    }
}
