using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareY.Database;

namespace ShareY.Controllers
{
    [Route(""), ApiController, AllowAnonymous]
    public class IndexController : Controller
    {
        private readonly ShareYContext _dbContext;

        public IndexController(ShareYContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route(""), HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Route("{name}"), HttpGet]
        public IActionResult GetFile(string name)
        {
            var file = _dbContext.Uploads.FirstOrDefault(x => x.FileName == name);

            if (file is null || !System.IO.File.Exists($"./uploads/{file.FileName}"))
            {
                return Redirect("/");
            }

            var fileBytes = System.IO.File.ReadAllBytes($"./uploads/{file.FileName}");

            return File(fileBytes, file.ContentType);
        }
    }
}
