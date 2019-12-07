using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PsychicPotato.Configurations;
using PsychicPotato.Database;
using PsychicPotato.Database.Models;
using PsychicPotato.Interfaces;

namespace PsychicPotato.Controllers
{
    [Route("api/[controller]"), ApiController, Authorize]
    public class UploadController : Controller
    {
        private readonly PsychicPotatoContext _dbContext;
        private readonly FilesConfiguration _filesConfiguration;

        public UploadController(PsychicPotatoContext dbContext, IFilesConfigurationProvider filesConfiguration)
        {
            _dbContext = dbContext;
            _filesConfiguration = filesConfiguration.GetConfiguration();
        }

        [HttpDelete, Route("cleanup"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> CleanupUploads(int days)
        {
            var dateTime = DateTime.Now - TimeSpan.FromDays(days);

            var files = _dbContext.Uploads;
            var fileNames = new List<string>();

            foreach (var file in files)
            {
                if (file.LastSeen > dateTime)
                {
                    continue;
                }

                file.Removed = true;
                _dbContext.Uploads.Update(file);

                System.IO.File.Delete($"./uploads/{file.FileName}");
            }

            await _dbContext.SaveChangesAsync();
            return Json(new { FilesDeleted = fileNames.Count, FileNames = fileNames });
        }

        [HttpGet, Route("{name}")]
        public IActionResult DetailsUpload(string name)
        {
            var file = _dbContext.Uploads.FirstOrDefault(x => x.FileName == name);

            if (file is null)
            {
                return NotFound("File not found.");
            }

            if (file.Removed)
            {
                return BadRequest("File is removed.");
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

        [HttpDelete, Route("{name}")]
        public async Task<IActionResult> DeleteUpload(string name)
        {
            var file = _dbContext.Uploads.FirstOrDefault(x => x.FileName == name);

            if (file is null)
            {
                return NotFound("File not found.");
            }

            if (file.AuthorGuid != Guid.Parse(HttpContext.User.Identity.Name))
            {
                return BadRequest("This file doesn't belong to you.");
            }

            var path = $"./uploads/{file.FileName}";

            if (file.Removed)
            {
                return BadRequest("File is already removed.");
            }

            if (!System.IO.File.Exists(path))
            {
                file.Removed = true;
                _dbContext.Uploads.Update(file);
                await _dbContext.SaveChangesAsync();

                return NotFound("File not found. File has just been marked as removed.");
            }

            System.IO.File.Delete(path);
            file.Removed = true;
            _dbContext.Uploads.Update(file);

            await _dbContext.SaveChangesAsync();

            return Ok("File has been successfully removed.");
        }

        [HttpPost, Route(""), DisableRequestSizeLimit]
        public async Task<IActionResult> PostUpload()
        {
            foreach (var file in Request.Form.Files)
            {
                if (file is null)
                {
                    return BadRequest();
                }

                if (file.Length > _filesConfiguration.SizeMax)
                {
                    return BadRequest($"Size of the file too big. ({_filesConfiguration.SizeMax}B max)");
                }

                if (file.Length < _filesConfiguration.SizeMin)
                {
                    return BadRequest($"Size of the file too low. ({_filesConfiguration.SizeMin}B min)");
                }

                var extension = Path.GetExtension(file.FileName);

                if (!_filesConfiguration.FileExtensions.Any(x => x == extension.Replace(".", "")))
                {
                    return BadRequest(new { Message = "Unsupported file extension.", _filesConfiguration.FileExtensions });
                }
            }

            var uploads = new List<object>();
            foreach (var file in Request.Form.Files)
            {
                var extension = Path.GetExtension(file.FileName);
                var filename = $"{Guid.NewGuid().ToString().Replace("-", "")}{extension}";

                var upload = new Upload
                {
                    AuthorGuid = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value),
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

                uploads.Add(new { Filename = filename, Type = upload.ContentType });
            }

            return Json(uploads);
        }
    }
}
