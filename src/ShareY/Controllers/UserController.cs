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
    [Route("api/[controller]"), ApiController, AllowAnonymous]
    public class UserController : Controller
    {
        private readonly ShareYContext _dbContext;

        public UserController(ShareYContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost, Route("create")]
        public async Task<IActionResult> CreateUser(UserCreateModel user)
        {
            var users = _dbContext.Users;

            if (users.Any(x => x.Email == user.Email))
            {
                return Json(new { Status = "403", Message = "A user with that email already exist." });
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

            return Ok();
        }
    }
}
