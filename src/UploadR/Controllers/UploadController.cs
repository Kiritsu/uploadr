using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UploadR.Enums;
using UploadR.Models;
using UploadR.Services;

namespace UploadR.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class UploadController : UploadRController
    {
        private readonly UploadService _uploadService;

        /// <summary>
        ///     Controller related to uploads management.
        /// </summary>
        /// <param name="uploadService">Service for uploads management.</param>
        public UploadController(
            UploadService uploadService)
        {
            _uploadService = uploadService;
        }
        
        /// <summary>
        ///     Upload the given files to the server. Returns a model representing failed and succeeded uploads.
        /// </summary>
        /// <param name="model">Model containing the password to put on every upload. Null if no password.</param>
        [HttpPost, Authorize, DisableRequestSizeLimit]
        public async Task<IActionResult> UploadAsync(
            [FromForm] UploadModel model)
        {
            return Json(await _uploadService.CreateAsync(
                UserGuid, Request.Form.Files, model.Password, model.ExpireAfter));
        }

        /// <summary>
        ///     Deletes the upload by its name or guid.
        /// </summary>
        /// <param name="filename">Complete name of the file or its guid.</param>
        [HttpDelete("{filename}"), Authorize]
        public async Task<IActionResult> DeleteAsync(
            string filename)
        {
            var result = await _uploadService.DeleteAsync(UserGuid, filename);

            return result switch
            {
                ResultCode.Ok => Ok(),
                ResultCode.NotFound => BadRequest(),
                ResultCode.Unauthorized => Unauthorized(),
                _ => BadRequest()
            };
        }

        /// <summary>
        ///     Deletes all the given uploads by their GUIDs.
        /// </summary>
        /// <param name="uploadIds">Ids of the uploads to delete. Limited to 100.</param>
        [HttpDelete, Authorize]
        public async Task<IActionResult> DeleteBulkAsync(
            string[] uploadIds)
        {
            var result = await _uploadService.DeleteBulkAsync(UserGuid, uploadIds);
            if (result is null)
            {
                return BadRequest();
            }
            
            return Json(result);
        }

        /// <summary>
        ///     Gets the details of an upload.
        /// </summary>
        /// <param name="filename">Complete name of the file or its guid.</param>
        [HttpGet("{filename}/details"), Authorize]
        public async Task<IActionResult> GetDetailsAsync(
            string filename)
        {
            var result = await _uploadService.GetDetailsAsync(filename);
            if (result is null)
            {
                return NotFound();
            }
            
            return Json(result);
        }

        /// <summary>
        ///     Gets the details of all the uploads sent by the specified user.
        /// </summary>
        /// <param name="userId">Id of the user to lookup.</param>
        /// <param name="limit">Amount of uploads to lookup.</param>
        /// <param name="afterId">Guid that defines the start of the query.</param>
        [HttpGet("{userId}/uploads"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserDetailsBulkAsync(
            string userId,
            [FromQuery(Name = "limit")] int limit = 100,
            [FromQuery(Name = "afterGuid")] string afterId = null)
        {
            if (!Guid.TryParse(userId, out var userGuid) 
                || !Guid.TryParse(afterId, out var afterGuid))
            {
                return BadRequest();
            }
            
            var result = await _uploadService.GetDetailsBulkAsync(userGuid, limit, afterGuid);
            if (result is null)
            {
                return BadRequest();
            }
            
            return Json(result);
        }
        
        /// <summary>
        ///     Gets the details of all the uploads sent by the current user.
        /// </summary>
        /// <param name="limit">Amount of uploads to lookup.</param>
        /// <param name="afterId">Guid that defines the start of the query.</param>
        [HttpGet("uploads"), Authorize]
        public async Task<IActionResult> GetUserDetailsBulkAsync(
            [FromQuery(Name = "limit")] int limit = 100,
            [FromQuery(Name = "afterGuid")] string afterId = null)
        {
            if (!Guid.TryParse(afterId, out var afterGuid))
            {
                return BadRequest();
            }
            
            var result = await _uploadService.GetDetailsBulkAsync(UserGuid, limit, afterGuid);
            if (result is null)
            {
                return BadRequest();
            }
            
            return Json(result);
        }

        /// <summary>
        ///     Gets an upload.
        /// </summary>
        /// <param name="filename">Complete name of the file or its guid.</param>
        /// <param name="password">Password of the file, if any is set.</param>
        [HttpGet("{filename}")]
        public async Task<IActionResult> GetAsync(
            string filename, 
            [FromQuery(Name = "password")] string password = null)
        {
            var (content, type) = await _uploadService.GetAsync(filename, password);

            if (content.Length != 0)
            {
                return File(content, type);
            }
            
            if (string.IsNullOrWhiteSpace(type))
            {
                return NotFound();
            }

            return Unauthorized();
        }
    }
}