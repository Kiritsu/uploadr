using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UploadR.Enums;
using UploadR.Models;
using UploadR.Services;

namespace UploadR.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class UploadController : Controller
    {
        private readonly UploadService _uploadService;

        /// <summary>
        ///     Controller related to uploads management.
        /// </summary>
        /// <param name="uploadService">Service for uploads management.</param>
        public UploadController(UploadService uploadService)
        {
            _uploadService = uploadService;
        }
        
        /// <summary>
        ///     Upload the given files to the server. Returns a model representing failed and succeeded uploads.
        /// </summary>
        /// <param name="model">Model containing the password to put on every upload. Null if no password.</param>
        [HttpPost, Authorize]
        public async Task<IActionResult> UploadAsync(
            [FromForm] UploadModel model)
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            return Json(await _uploadService.UploadAsync(userId?.Value, Request.Form.Files, model.Password));
        }

        /// <summary>
        ///     Deletes the upload by its name or guid.
        /// </summary>
        /// <param name="filename">Complete name of the file or its guid.</param>
        [HttpDelete("{filename}"), Authorize]
        public async Task<IActionResult> DeleteUploadAsync(string filename)
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            var result = await _uploadService.DeleteUploadAsync(userId?.Value, filename);

            return result switch
            {
                ResultCode.Ok => Ok(),
                ResultCode.NotFound => BadRequest(),
                ResultCode.Unauthorized => Unauthorized(),
                _ => BadRequest()
            };
        }

        /// <summary>
        ///     Gets the details of an upload.
        /// </summary>
        /// <param name="filename">Complete name of the file or its guid.</param>
        [HttpGet("{filename}/details"), Authorize]
        public async Task<IActionResult> GetUploadDetailsAsync(string filename)
        {
            return Json(await _uploadService.GetUploadDetailsAsync(filename));
        }
        
        /// <summary>
        ///     Gets the details of an upload.
        /// </summary>
        /// <param name="filename">Complete name of the file or its guid.</param>
        [HttpGet("{filename}")]
        public async Task<IActionResult> GetUploadContentAsync(string filename)
        {
            var (content, type) = await _uploadService.GetUploadAsync(filename);

            if (content.Length == 0 && string.IsNullOrWhiteSpace(type))
            {
                return NotFound();
            }
            
            return File(content, type);
        }
    }
}