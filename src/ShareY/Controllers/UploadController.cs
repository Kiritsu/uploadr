using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShareY.Database;
using ShareY.Database.Models;

namespace ShareY.Controllers
{
    [Route("api/[controller]"), ApiController, Authorize]
    public class UploadController : Controller
    {
        private readonly ShareYContext _dbContext;

        public UploadController(ShareYContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> PostUpload()
        {
            var file = Request.Form.Files.FirstOrDefault();

            if (file is null)
            {
                return BadRequest();
            }

            var extension = Path.GetExtension(file.FileName);
            var filename = $"{GetRandomName()}{extension}";
            while (_dbContext.Uploads.Any(x => x.FileName == filename))
            {
                filename = $"{GetRandomName()}{extension}";
            }

            var upload = new Upload
            {
                AuthorId = HttpContext.User.Identity.Name,
                CreatedAt = DateTime.Now,
                DownloadCount = 0,
                Removed = false,
                Visible = true,
                Id = Guid.NewGuid().ToString(),
                FileName = filename,
                ContentType = file.ContentType
            };

            await _dbContext.Uploads.AddAsync(upload);
            await _dbContext.SaveChangesAsync();

            if (!Directory.Exists("./uploads"))
            {
                Directory.CreateDirectory("./uploads");
            }

            using (var fs = System.IO.File.Create($"./uploads/{filename}"))
            {
                await file.CopyToAsync(fs);
            }

            return Redirect($"/{filename}");
        }

        private static string GetRandomName()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random(DateTime.Now.Millisecond);
            return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
