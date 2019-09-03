using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareY.Attributes;
using ShareY.Database;
using ShareY.Models;

namespace ShareY.Controllers
{
    [Route("admin"), RequiresAdminAuthentication]
    public class AdminController : ShareYController
    {
        public AdminController(ShareYContext dbContext) : base(dbContext)
        {
        }

        [Route(""), HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Route("uploads"), HttpGet]
        public IActionResult Uploads()
        {
            return View();
        }

        [Route("users"), HttpGet]
        public IActionResult Users()
        {
            var model = new AdminUsersViewModel
            {
                Users = _dbContext.Users
                    .Include(x => x.Token)
                    .ToList()
            };
            return View(model);
        }

        [Route("settings"), HttpGet]
        public IActionResult Settings()
        {
            return View();
        }
    }
}
