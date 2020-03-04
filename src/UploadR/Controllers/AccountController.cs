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

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

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
    }
}
