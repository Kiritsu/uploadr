using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UploadR.Authentications;
using UploadR.Enums;
using UploadR.Models;
using UploadR.Services;

namespace UploadR.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;

        /// <summary>
        ///     Controller related to accounts management.
        /// </summary>
        /// <param name="accountService">Service for accounts management.</param>
        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        ///     Route to reset the current user's api token.
        /// </summary>
        [HttpPatch("reset"), Authorize]
        public async Task<IActionResult> ResetTokenAsync()
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            var result = await _accountService.ResetUserTokenAsync(userId?.Value);

            return result switch
            {
                ResultCode.Ok => Ok(),
                _ => BadRequest()
            };
        }

        /// <summary>
        ///    Route to reset a user's api token. 
        /// </summary>
        /// <param name="userId">User id to reset.</param>
        [HttpPatch("{userId}/reset"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetUserTokenAsync(string userId)
        {
            var result = await _accountService.ResetUserTokenAsync(userId);
            
            return result switch
            {
                ResultCode.Ok => Ok(),
                ResultCode.Invalid => Unauthorized(),
                _ => BadRequest()
            };
        }
        
        /// <summary>
        ///    Route to block a user. 
        /// </summary>
        /// <param name="userId">User id to block.</param>
        [HttpPatch("{userId}/block"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> BlockAccountAsync(string userId)
        {
            var result = await _accountService.ToggleAccountStateAsync(userId, true);
            
            return result switch
            {
                ResultCode.Ok => Ok(),
                ResultCode.Invalid => Unauthorized(),
                _ => BadRequest()
            };
        }
        
        /// <summary>
        ///    Route to unblock a user. 
        /// </summary>
        /// <param name="userId">User id to unblock.</param>
        [HttpPatch("{userId}/unblock"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnblockAccountAsync(string userId)
        {
            var result = await _accountService.ToggleAccountStateAsync(userId, false);
            
            return result switch
            {
                ResultCode.Ok => Ok(),
                ResultCode.Invalid => Unauthorized(),
                _ => BadRequest()
            };
        }

        /// <summary>
        ///     Route to delete the current authenticated account.
        /// </summary>
        /// <param name="cascade">Whether to delete or not the uploads made by that account.</param>
        [HttpDelete("{cascade}"), Authorize]
        public async Task<IActionResult> DeleteAccountAsync([FromQuery] bool cascade = false)
        {
            var token = User.Claims.FirstOrDefault(
                x => x.Type == TokenAuthenticationHandler.ClaimToken);
            var result = await _accountService.DeleteAccountAsync(token?.Value, cascade);

            return result switch
            {
                ResultCode.Ok => Ok(),
                _ => BadRequest()
            };
        }

        /// <summary>
        ///    Route to create an account from an email passed in the request body. 
        /// </summary>
        /// <param name="accountCreateModel">Account creation model.</param>
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> CreateAccountAsync(
            [FromForm] AccountCreateModel accountCreateModel)
        {
            if (string.IsNullOrWhiteSpace(accountCreateModel.Email))
            {
                return BadRequest();
            }

            var result = await _accountService.CreateAccountAsync(accountCreateModel.Email);

            return result switch
            {
                ResultCode.Ok => Ok(),
                ResultCode.EmailInUse => Forbid(),
                _ => BadRequest()
            };
        }

        /// <summary>
        ///     Route to verify an account.
        /// </summary>
        [HttpPost("verify"), Authorize(Roles = "Unverified")]
        public async Task<IActionResult> VerifyAccountAsync()
        {
            var token = User.Claims.FirstOrDefault(
                x => x.Type == TokenAuthenticationHandler.ClaimToken);
            var result = await _accountService.VerifyAccountAsync(token?.Value);

            return result ? (IActionResult) Ok() : BadRequest();
        }
    }
}
