using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UploadR.Enum;
using UploadR.Services;

namespace UploadR.Controllers
{
    [Route("api/[controller]"), ApiController, Authorize]
    public class UploadController : Controller
    {
        private readonly UploadsService _uploads;

        public UploadController(UploadsService uploads)
        {
            _uploads = uploads;
        }

        [HttpDelete, Route("cleanup"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> CleanupUploads(int days)
        {
            var removedFiles = await _uploads.CleanupAsync(TimeSpan.FromDays(days));
            return Json(new { removedFiles.Count, FileNames = removedFiles });
        }

        [HttpGet, Route("{name}")]
        public async Task<IActionResult> DetailsUpload(string name)
        {
            var file = await _uploads.TryGetUploadByNameAsync(name);
            if (file.IsSuccess)
            {
                return Ok(new
                {
                    File = new
                    {
                        Name = file.Value.FileName,
                        Url = $"{Request.Scheme}://{Request.Host.Value}/{file.Value.FileName}",
                        Views = file.Value.DownloadCount,
                        Author = file.Value.AuthorGuid,
                        FileType = file.Value.ContentType,
                        UploadedAt = file.Value.CreatedAt,
                        IsRemoved = file.Value.Removed
                    }
                });
            }

            return file.Code switch
            {
                ResultErrorType.NotFound => NotFound(new { Reason = "Unknown File.", file.Code }),
                ResultErrorType.Removed => NotFound(new { Reason = "File is removed.", file.Code }),
                ResultErrorType.NotFoundRemoved => NotFound(new { Reason = "File not found. It has been marked as removed.", file.Code }),
                _ => BadRequest()
            };
        }

        [HttpDelete, Route("{name}")]
        public async Task<IActionResult> DeleteUpload(string name)
        {
            var result = await _uploads.RemoveAsync(name, Guid.Parse(HttpContext.User.Identity.Name));
            if (result.IsSuccess)
            {
                return Ok();
            }

            return result.Code switch
            {
                ResultErrorType.NotFound => NotFound(new { Reason = "Unknown File.", result.Code }),
                ResultErrorType.Removed => NotFound(new { Reason = "File is removed.", result.Code }),
                ResultErrorType.NotFoundRemoved => NotFound(new { Reason = "File not found. It has been marked as removed.", result.Code }),
                ResultErrorType.Unauthorized => Unauthorized(new { Reason = "This file doesn't belong to you.", result.Code }),
                _ => BadRequest()
            };
        }

        [HttpPost, Route(""), DisableRequestSizeLimit]
        public async Task<IActionResult> PostUpload([FromQuery(Name = "p")] string password)
        {
            // Silently ignore when filecount > 1 and doesn't match configuration.
            var files = new List<IFormFile>();
            if (Request.Form.Files.Count == 1)
            {
                var file = Request.Form.Files.First();
                var result = _uploads.IsValidFile(file);
                if (result.IsSuccess)
                {
                    files.Add(file);
                }
                else
                {
                    return result.Code switch
                    {
                        ResultErrorType.Null => BadRequest(),
                        ResultErrorType.TooBig => BadRequest(new { Reason = "Too big.", result.Code }),
                        ResultErrorType.TooSmall => BadRequest(new { Reason = "Too small.", result.Code }),
                        ResultErrorType.UnsupportedFileExtension => BadRequest(new { Reason = "Unsupported extension.", result.Code }),
                        _ => BadRequest()
                    };
                }
            }
            else
            {
                files = _uploads.FilterBadFiles(Request.Form.Files).ToList();
            }

            var authorGuid = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value);
            var uploads = new List<object>();
            foreach (var file in Request.Form.Files)
            {
                var result = await _uploads.UploadFileAsync(authorGuid, file, password);
                uploads.Add(new { Filename = result.Value.FileName, Type = result.Value.ContentType });
            }

            return Json(uploads);
        }
    }
}
