using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ShareY.Attributes;
using ShareY.Database;
using ShareY.Database.Enums;
using ShareY.Database.Models;
using ShareY.Extensions;

namespace ShareY.Controllers
{
    public abstract class ShareYController : Controller
    {
        protected readonly ShareYContext _dbContext;

        public Guid? UserToken { get; private set; }

        public User DbUser { get; private set; }

        public ShareYController(ShareYContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await base.OnActionExecutionAsync(context, next);

            UserToken = context.HttpContext.Session.Get<Guid?>("userToken");

            DbUser = await _dbContext.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Token.Guid == UserToken);

            ViewData["IsAuthenticated"] = DbUser != null && !DbUser.Disabled && !DbUser.Token.Revoked;
            ViewData["IsAdmin"] = DbUser != null && !DbUser.Disabled && !DbUser.Token.Revoked && DbUser.Token.TokenType == TokenType.Admin;

            if (DbUser != null)
            {
                ViewData["UserToken"] = DbUser.Token.Guid.ToString();
            }

            if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
            {
                var controller = actionDescriptor.ControllerTypeInfo;
                var method = actionDescriptor.MethodInfo;

                var attributes = controller.CustomAttributes.ToList();
                attributes.AddRange(method.CustomAttributes);

                if (attributes.Any(x => x.AttributeType == typeof(RequiresAdminAuthentication)) && !(bool)ViewData["IsAdmin"])
                {
                    throw new Exception("Admin authentication required.");
                }

                if (attributes.Any(x => x.AttributeType == typeof(RequiresAuthentication)) && !(bool)ViewData["IsAuthenticated"])
                {
                    throw new Exception("Authentication required.");
                }
            }
        }
    }
}
