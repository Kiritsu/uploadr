using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> CreateAccountAsync(
            [FromBody] AccountCreateModel accountCreateModel)
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
        /// <param name="token">Token associated with the account to verify.</param>
        [HttpGet("verify/{token}")]
        [Authorize("Unverified")]
        public async Task<IActionResult> VerifyAccountAsync(string token)
        {
            var result = await _accountService.VerifyAccountAsync(token);

            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}
