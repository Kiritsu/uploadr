using System;
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
    public class AccountController : UploadRController
    {
        private readonly AccountService _accountService;

        /// <summary>
        ///     Controller related to accounts management.
        /// </summary>
        /// <param name="accountService">Service for accounts management.</param>
        public AccountController(
            AccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        ///     Route to reset the current user's api token.
        /// </summary>
        [HttpPatch("token"), Authorize]
        public async Task<IActionResult> ResetTokenAsync()
        {
            var result = await _accountService.ResetUserTokenAsync(UserGuid);

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
        [HttpPatch("{userId}/token"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetUserTokenAsync(
            Guid? userId)
        {
            if (!userId.HasValue)
            {
                return BadRequest();
            }
            
            var result = await _accountService.ResetUserTokenAsync(userId.Value);
            
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
        public async Task<IActionResult> BlockAccountAsync(
            Guid? userId)
        {
            if (!userId.HasValue)
            {
                return BadRequest();
            }

            var result = await _accountService.ToggleAccountStateAsync(userId.Value, true);

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
        public async Task<IActionResult> UnblockAccountAsync(
            Guid? userId)
        {
            if (!userId.HasValue)
            {
                return BadRequest();
            }

            var result = await _accountService.ToggleAccountStateAsync(userId.Value, false);
            
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
        [HttpDelete, Authorize]
        public async Task<IActionResult> DeleteAccountAsync()
        {
            var result = await _accountService.DeleteAccountAsync(UserGuid);

            return result switch
            {
                ResultCode.Ok => Ok(),
                ResultCode.Unauthorized => Unauthorized(),
                _ => BadRequest()
            };
        }
        
        /// <summary>
        ///     Route to delete the current authenticated account.
        /// </summary>
        [HttpDelete("{userId}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAccountAsync(
            Guid? userId)
        {
            if (!userId.HasValue)
            {
                return BadRequest();
            }
            
            var result = await _accountService.DeleteAccountAsync(userId.Value);

            return result switch
            {
                ResultCode.Ok => Ok(),
                ResultCode.Unauthorized => Unauthorized(),
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
            var result = await _accountService.CreateAccountAsync(accountCreateModel);

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
            var token = User.FindFirstValue(TokenAuthenticationHandler.ClaimToken);
            var result = await _accountService.VerifyAccountAsync(token);

            return result ? (IActionResult) Ok() : BadRequest();
        }
    }
}
