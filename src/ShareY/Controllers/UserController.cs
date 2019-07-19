using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareY.Database;
using ShareY.Database.Models;
using ShareY.Models;

namespace ShareY.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class UserController : Controller
    {
        private readonly ShareYContext _dbContext;

        public UserController(ShareYContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpDelete, Route("delete"), Authorize]
        public async Task<IActionResult> DeleteUser()
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Guid == Guid.Parse(HttpContext.User.Identity.Name));

            _dbContext.Remove(user);

            await _dbContext.SaveChangesAsync();

            return Json(new { Status = 200, Message = "This account has been removed." });
        }

        [HttpPost, Route("create"), AllowAnonymous]
        public async Task<IActionResult> CreateUser(UserCreateModel user)
        {
            if (user is null || string.IsNullOrWhiteSpace(user.Email))
            {
                return BadRequest();
            }

            var users = _dbContext.Users;

            if (users.Any(x => x.Email == user.Email))
            {
                return Json(new { Status = 403, Message = "A user with that email already exist." });
            }

            var dbUser = new User
            {
                Guid = Guid.NewGuid(),
                Email = user.Email,
                CreatedAt = DateTime.Now,
                Disabled = false
            };

            var dbToken = new Token
            {
                CreatedAt = DateTime.Now,
                Guid = Guid.NewGuid(),
                UserGuid = dbUser.Guid
            };

            await _dbContext.AddAsync(dbUser);
            await _dbContext.AddAsync(dbToken);

            await _dbContext.SaveChangesAsync();

            return Json(new { Status = 200, Message = "Account created.", Token = dbToken.Guid });
        }
    }
}
