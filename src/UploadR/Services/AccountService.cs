using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UploadR.Database;
using UploadR.Database.Enums;
using UploadR.Database.Models;
using UploadR.Enums;
using UploadR.Utilities;

namespace UploadR.Services
{
    public class AccountService
    {
        public IServiceProvider Services { get; }

        public AccountService(IServiceProvider services)
        {
            Services = services;
        }

        public async Task<ResultCode> CreateAccountAsync(string email)
        {
            using var scope = Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = Services.GetRequiredService<UploadRContext>();

            if (!RegexUtilities.IsValidEmail(email))
            {
                return ResultCode.Invalid;
            }

            if (await db.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower()))
            {
                return ResultCode.EmailInUse;
            }

            await db.Users.AddAsync(new User
            {
                Guid = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Disabled = false,
                Email = email,
                Verified = false,
                Type = AccountType.User,
                Token = Guid.NewGuid().ToString()
            });

            await db.SaveChangesAsync();

            return ResultCode.Ok;
        }
    }
}
