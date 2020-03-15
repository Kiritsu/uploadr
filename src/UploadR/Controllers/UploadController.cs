using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    }
}