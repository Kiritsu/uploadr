using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShareY.Configurations;
using ShareY.Database;
using ShareY.Database.Models;
using ShareY.Interfaces;

namespace ShareY.Controllers
{
    [Route("api/[controller]"), ApiController, Authorize]
    public class UploadController : Controller
    {
        private readonly ShareYContext _dbContext;
        private readonly FilesConfiguration _filesConfiguration;

        public UploadController(ShareYContext dbContext, IFilesConfigurationProvider filesConfiguration)
        {
            _dbContext = dbContext;
            _filesConfiguration = filesConfiguration.GetConfiguration();
        }

        [HttpGet, Route("details/{name}")]
        public IActionResult DetailsUpload(string name)
        {
            var file = _dbContext.Uploads.FirstOrDefault(x => x.FileName == name);

            if (file is null)
            {
                return NotFound(new { Message = "File not found." });
            }

            if (file.Removed)
            {
                return BadRequest(new { Message = "File is removed." });
            }

            return Ok(
                new
                {
                    File = new
                    {
                        Name = file.FileName,
                        Url = $"{Request.Scheme}://{Request.Host.Value}/{file.FileName}",
                        Views = file.ViewCount,
                        Author = file.AuthorGuid,
                        FileType = file.ContentType,
                        UploadedAt = file.CreatedAt,
                        IsRemoved = file.Removed
                    }
                });
        }

        [HttpDelete, Route("delete/{name}")]
        public async Task<IActionResult> DeleteUpload(string name)
        {
            var file = _dbContext.Uploads.FirstOrDefault(x => x.FileName == name);

            if (file is null)
            {
                return NotFound(new { Message = "File not found." });
            }

            if (file.AuthorGuid != Guid.Parse(HttpContext.User.Identity.Name))
            {
                return BadRequest(new { Message = "This file doesn't belong to you." });
            }

            var path = $"./uploads/{file.FileName}";

            if (file.Removed)
            {
                return BadRequest(new { Message = "File is already removed." });
            }

            if (!System.IO.File.Exists(path))
            {
                file.Removed = true;
                _dbContext.Uploads.Update(file);
                await _dbContext.SaveChangesAsync();

                return NotFound(new { Message = "File not found. File has just been marked as removed." });
            }

            System.IO.File.Delete(path);
            file.Removed = true;
            _dbContext.Uploads.Update(file);

            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "File has been successfully removed." });
        }

        [HttpPost]
        public async Task<IActionResult> PostUpload()
        {
            var file = Request.Form.Files.FirstOrDefault();

            if (file is null)
            {
                return BadRequest();
            }

            if (file.Length > _filesConfiguration.SizeMax)
            {
                return BadRequest(new { Message = $"Size of the file too big. ({_filesConfiguration.SizeMax}B max)" });
            }

            if (file.Length < _filesConfiguration.SizeMin)
            {
                return BadRequest(new { Message = $"Size of the file too low. ({_filesConfiguration.SizeMin}B min)" });
            }

            var extension = Path.GetExtension(file.FileName);

            if (!_filesConfiguration.FileExtensions.Any(x => x == extension.Replace(".", "")))
            {
                return BadRequest(new { Message = "Unsupported file extension.", FileExtensions = _filesConfiguration.FileExtensions });
            }

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

            return Ok(new { Filename = filename }); //return Redirect($"/{filename}");
        }

        private static string GetRandomName()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random(DateTime.Now.Millisecond);
            return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
