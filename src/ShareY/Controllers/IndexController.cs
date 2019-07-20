﻿using System.Linq;
using System.Threading.Tasks;
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
        public async Task<IActionResult> GetFile(string name)
        {
            var file = _dbContext.Uploads.FirstOrDefault(x => x.FileName == name);

            if (file is null)
            {
                return Redirect("/");
            }

            var path = $"./uploads/{file.FileName}";

            if (file.Removed)
            {
                return NotFound(new { Message = "File is removed."  });
            }

            if (!System.IO.File.Exists(path))
            {
                file.Removed = true;
                _dbContext.Uploads.Update(file);
                await _dbContext.SaveChangesAsync();

                return NotFound(new { Message = "File not found. File has just been marked as removed." });
            }

            var fileBytes = System.IO.File.ReadAllBytes(path);

            file.ViewCount++;
            _dbContext.Uploads.Update(file);
            await _dbContext.SaveChangesAsync();

            return File(fileBytes, file.ContentType);
        }
    }
}