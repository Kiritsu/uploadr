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
    public class ShortenController : UploadRController
    {
        private readonly ShortenService _shortenService;

        /// <summary>
        ///     Controller related to shorten management.
        /// </summary>
        /// <param name="shortenService">Service for shorten management.</param>
        public ShortenController(
            ShortenService shortenService)
        {
            _shortenService = shortenService;
        }

        /// <summary>
        ///     Creates a new shortened url from the model passed in form.
        /// </summary>
        /// <param name="model">Model containing different information for the url to shorten.</param>
        [HttpPost, Authorize]
        public async Task<IActionResult> ShortenAsync(
            [FromForm] ShortenModel model)
        {
            return Json(await _shortenService.ShortenAsync(
                UserGuid, model.Url, model.Proposal, model.Password, model.ExpireAfter));
        }

        /// <summary>
        ///     Deletes the shortened url by its name or id.
        /// </summary>
        /// <param name="shortenId">Id or name of the shortened url.</param>
        [HttpDelete("{shortenId}"), Authorize]
        public async Task<IActionResult> DeleteAsync(
            string shortenId)
        {
            var result = await _shortenService.DeleteAsync(UserGuid, shortenId);
            
            return result switch
            {
                ResultCode.Ok => Ok(),
                ResultCode.NotFound => BadRequest(),
                ResultCode.Unauthorized => Unauthorized(),
                _ => BadRequest()
            };
        }

        /// <summary>
        ///     Gets the details of a shortened url by its name or id.
        /// </summary>
        /// <param name="shortenId">Id or name of the shortened url.</param>
        [HttpGet("{shortenId}/details"), Authorize]
        public async Task<IActionResult> GetDetailsAsync(
            string shortenId)
        {
            var result = await _shortenService.GetDetailsAsync(shortenId);
            if (result is null)
            {
                return NotFound();
            }

            return Json(result);
        }
        
        /// <summary>
        ///     Gets the details of all the shortened urls created by the specified user.
        /// </summary>
        /// <param name="userId">Id of the user to lookup.</param>
        /// <param name="limit">Amount of shortened urls to lookup.</param>
        /// <param name="afterId">Guid that defines the start of the query.</param>
        [HttpGet("{userId}/shortens"), Authorize(Roles = "Admin")]
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
            
            var result = await _shortenService.GetDetailsBulkAsync(userGuid, limit, afterGuid);
            if (result is null)
            {
                return BadRequest();
            }
            
            return Json(result);
        }
        
        /// <summary>
        ///     Redirects to a shortened url.
        /// </summary>
        /// <param name="shortenId">Complete name of the file or its guid.</param>
        /// <param name="password">Password of the file, if any is set.</param>
        [HttpGet("{shortenId}")]
        public async Task<IActionResult> GetAsync(
            string shortenId, 
            [FromQuery(Name = "password")] string password = null)
        {
            var (content, url) = await _shortenService.GetAsync(shortenId, password);

            return content switch
            {
                ResultCode.NotFound => NotFound(),
                ResultCode.Unauthorized => Unauthorized(),
                ResultCode.Ok => Redirect(url),
                _ => BadRequest()
            };
        }
    }
}