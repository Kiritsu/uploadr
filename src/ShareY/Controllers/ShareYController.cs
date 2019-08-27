using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShareY.Database;
using ShareY.Database.Enums;
using ShareY.Database.Models;
using ShareY.Extensions;
using ShareY.Models;

namespace ShareY.Controllers
{
    public class ShareYController : Controller
    {
        private BaseViewModel Model => new BaseViewModel { IsAuthenticated = IsAuthenticated, UserToken = UserToken, IsAdmin = IsAdmin };

        protected readonly ShareYContext _dbContext;

        public Guid? UserToken => HttpContext?.Session?.Get<Guid?>("userToken");

        public bool IsAuthenticated => DbUser != null;

        public bool IsAdmin => DbUser != null && DbUser.Token.TokenType == TokenType.Admin;

        public User DbUser => _dbContext.Users.Include(x => x.Token).FirstOrDefault(x => x.Token.Guid == UserToken);

        public ShareYController(ShareYContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override ViewResult View()
        {
            return base.View(Model);
        }
    }
}
