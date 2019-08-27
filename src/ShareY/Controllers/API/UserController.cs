using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareY.Configurations;
using ShareY.Database;
using ShareY.Database.Enums;
using ShareY.Database.Models;
using ShareY.Interfaces;
using ShareY.Models;

namespace ShareY.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class UserController : Controller
    {
        private readonly ShareYContext _dbContext;
        private readonly RoutesConfiguration _routesConfiguration;

        public UserController(ShareYContext dbContext, IRoutesConfigurationProvider routesConfiguration)
        {
            _dbContext = dbContext;
            _routesConfiguration = routesConfiguration.GetConfiguration();
        }

        [HttpPatch, Route("unblock/{guid}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnblockUser(string guid)
        {
            if (!Guid.TryParse(guid, out var userGuid))
            {
                return BadRequest("Invalid token supplied.");
            }

            var user = _dbContext.Users.FirstOrDefault(x => x.Guid == userGuid);
            if (user is null)
            {
                return BadRequest("Unknown token supplied.");
            }

            if (user.Token.TokenType == TokenType.Admin)
            {
                return BadRequest("Cannot modify admin token.");
            }

            user.Disabled = false;
            _dbContext.Update(user);

            await _dbContext.SaveChangesAsync();

            return Ok("User unblocked.");
        }

        [HttpPatch, Route("block/{guid}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> BlockUser(string guid)
        {
            if (!Guid.TryParse(guid, out var userGuid))
            {
                return BadRequest("Invalid token supplied.");
            }

            var user = _dbContext.Users.FirstOrDefault(x => x.Guid == userGuid);
            if (user is null)
            {
                return BadRequest("Unknown token supplied.");
            }

            if (user.Token.TokenType == TokenType.Admin)
            {
                return BadRequest("Cannot modify admin token.");
            }

            user.Disabled = true;
            _dbContext.Update(user);

            await _dbContext.SaveChangesAsync();

            return Ok("User blocked.");
        }

        [HttpDelete, Route("delete"), Authorize]
        public async Task<IActionResult> DeleteUser()
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Guid == Guid.Parse(HttpContext.User.Identity.Name));

            _dbContext.Remove(user);

            await _dbContext.SaveChangesAsync();

            return Ok("This account has been removed.");
        }

        [HttpPost, Route("create"), AllowAnonymous]
        public async Task<IActionResult> CreateUser(UserCreateModel user)
        {
            if (!_routesConfiguration.UserRegisterRoute)
            {
                return Forbid();
            }

            if (user is null)
            {
                return BadRequest("Invalid form.");
            }

            var users = _dbContext.Users;

            if (!string.IsNullOrWhiteSpace(user.Email) && users.Any(x => x.Email == user.Email))
            {
                return BadRequest("A user with that email already exist.");
            }

            var dbUser = new User
            {
                Guid = Guid.NewGuid(),
                Email = user?.Email ?? "",
                CreatedAt = DateTime.Now,
                Disabled = false
            };

            var dbToken = new Token
            {
                CreatedAt = DateTime.Now,
                Guid = Guid.NewGuid(),
                UserGuid = dbUser.Guid,
                TokenType = TokenType.User,
                Revoked = false
            };

            await _dbContext.AddAsync(dbUser);
            await _dbContext.AddAsync(dbToken);

            await _dbContext.SaveChangesAsync();

            return Ok(new { Token = dbToken.Guid });
        }
    }
}
