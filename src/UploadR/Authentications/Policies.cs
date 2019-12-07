using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using UploadR.Database.Enums;

namespace UploadR.Authentications
{
    public sealed class AdminRequirement : IAuthorizationRequirement
    {
        public const string PolicyName = "RequireAdminPrivileges";
    }

    public sealed class AdminRequirementHandler : AuthorizationHandler<AdminRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            if (context.User.Identity.IsAuthenticated && context.User.IsInRole(TokenType.Admin.ToString()))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail(); // hard fail
            }

            return Task.CompletedTask;
        }
    }

    public sealed class UserRequirement : IAuthorizationRequirement
    {
        public const string PolicyName = "RequireUserPrivileges";
    }

    public sealed class UserRequirementHandler : AuthorizationHandler<UserRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRequirement requirement)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
