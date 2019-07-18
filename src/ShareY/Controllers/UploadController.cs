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

        [HttpDelete, Route("delete/{name}")]
        public async Task<IActionResult> DeleteUpload(string name)
        {
            var file = _dbContext.Uploads.FirstOrDefault(x => x.FileName == name);

            if (file is null)
            {
                return Json(new { Status = "404", Message = "File not found." });
            }

            if (file.AuthorGuid != Guid.Parse(HttpContext.User.Identity.Name))
            {
                return Json(new { Status = "403", Message = "This file doesn't belong to you." });
            }

            var path = $"./uploads/{file.FileName}";

            if (file.Removed)
            {
                return Json(new { Status = "304", Message = "File is already removed." });
            }

            if (!System.IO.File.Exists(path))
            {
                file.Removed = true;
                _dbContext.Uploads.Update(file);
                await _dbContext.SaveChangesAsync();

                return Json(new { Status = "404", Message = "File not found. File has just been marked as removed." });
            }

            System.IO.File.Delete(path);
            file.Removed = true;
            _dbContext.Uploads.Update(file);
            await _dbContext.SaveChangesAsync();

            return Json(new { Status = "200", Message = "File has been successfully removed." });
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
                AuthorGuid = Guid.Parse(HttpContext.User.Identity.Name),
                CreatedAt = DateTime.Now,
                ViewCount = 0,
                Removed = false,
                Guid = Guid.NewGuid(),
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
