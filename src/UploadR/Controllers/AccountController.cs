using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UploadR.Authentications;
using UploadR.Enums;
using UploadR.Models;
using UploadR.Services;

namespace UploadR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        ///    Route to create an account from an email passed in the request body. 
        /// </summary>
        /// <param name="accountCreateModel">Account creation model.</param>
        [HttpPost]
        [AllowAnonymous]
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
        [HttpPost("verify")]
        [Authorize(Roles = "Unverified")]
        public async Task<IActionResult> VerifyAccountAsync()
        {
            var token = User.Claims.FirstOrDefault(x => x.Type == TokenAuthenticationHandler.ClaimToken);
            var result = await _accountService.VerifyAccountAsync(token?.Value);

            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}
