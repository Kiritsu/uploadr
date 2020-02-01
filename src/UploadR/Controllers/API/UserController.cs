using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UploadR.Authentications;
using UploadR.Enum;
using UploadR.Models;
using UploadR.Services;

namespace UploadR.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class UserController : Controller
    {
        private readonly UserService _us;
        private readonly EmailService _emails;

        public UserController(UserService us, EmailService emails)
        {
            _us = us;
            _emails = emails;
        }

        [HttpDelete, Route("token"), Authorize]
        public async Task<IActionResult> ResetAsync([FromQuery] bool reset = false)
        {
            var guid = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value);
            var token = User.Claims.FirstOrDefault(x => x.Type == TokenAuthenticationHandler.ClaimToken);

            var result = await _us.ResetOrRevokeTokenAsync(guid, token.Value, reset);
            
            if (reset)
            {
                await _emails.SendTokenResetAsync(result.Value, result.Value.Token);
                return Json(new { Token = result.Value.Token, Revoked = false });
            }
            else
            {
                await _emails.SendCustomActionAsync(result.Value, "TOKEN_REVOKED");
                return Json(new { Token = "", Revoked = true });
            }
        }

        [HttpPatch, Route("{guid}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleBlockUser(string guid, [FromQuery] bool block)
        {
            var result = await _us.BlockOrUnblockUserAsync(guid, block);

            if (result.IsSuccess)
            {
                await _emails.SendCustomActionAsync(result.Value, block ? "ACCOUNT_BLOCKED" : "ACCOUNT_UNBLOCKED");
                return Json(new { UserGuid = result.Value.Guid.ToString(), Blocked = block });
            }

            return result.Code switch
            {
                ResultErrorType.InvalidGuid => BadRequest(new { Reason = "Invalid provided guid.", result.Code }),
                ResultErrorType.Null => BadRequest(new { Reason = "Couldn't retrieve any user from provided guid.", result.Code }),
                ResultErrorType.Unauthorized => BadRequest(new { Reason = "Cannot block/unblock any admin user.", result.Code }),
                _ => BadRequest()
            };
        }

        [HttpDelete, Route(""), Authorize]
        public async Task<IActionResult> DeleteUser()
        {
            var guid = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value);
            await _us.DeleteAccountAsync(guid);

            await _emails.SendCustomActionAsync(guid, "ACCOUNT_REMOVED");

            return Ok();
        }

        [HttpPost, Route(""), AllowAnonymous]
        public async Task<IActionResult> CreateUser(UserCreateModel user)
        {
            var result = await _us.CreateAccountAsync(user);
            
            if (result.IsSuccess)
            {
                return Json(new { result.Value.Token });
            }

            return result.Code switch
            {
                ResultErrorType.Unauthorized => Unauthorized(),
                ResultErrorType.EmailNotProvided => BadRequest(new { Reason = "An email is needed to create an account.", result.Code }),
                ResultErrorType.Found => BadRequest(new { Reason = "A user with that email already exists.", result.Code }),
                _ => BadRequest()
            };
        }
    }
}
