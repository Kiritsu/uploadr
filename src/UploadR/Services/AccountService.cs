using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UploadR.Database;
using UploadR.Database.Enums;
using UploadR.Database.Models;
using UploadR.Enums;
using UploadR.Utilities;

namespace UploadR.Services
{
    public class AccountService
    {
        private readonly IServiceProvider _services;
        private readonly SHA512Managed _sha512Managed;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IServiceProvider services, SHA512Managed sha512Managed,
            ILogger<AccountService> logger)
        {
            _services = services;
            _sha512Managed = sha512Managed;
            _logger = logger;
        }

        /// <summary>
        ///     Creates a new account with the given email.
        /// </summary>
        /// <param name="email">Email associated with the new account.</param>
        public async Task<ResultCode> CreateAccountAsync(string email)
        {
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            if (!RegexUtilities.IsValidEmail(email))
            {
                return ResultCode.Invalid;
            }

            if (await db.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower()))
            {
                return ResultCode.EmailInUse;
            }

            var blankToken = Guid.NewGuid();
            var byteHash = _sha512Managed.ComputeHash(blankToken.ToByteArray());
            var token = string.Join("", byteHash.Select(x => x.ToString("X2")));
            
            await db.Users.AddAsync(new User
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Disabled = false,
                Email = email,
                Type = AccountType.Unverified,
                Token = token
            });

            await db.SaveChangesAsync();

            _logger.Log(LogLevel.Debug, 
                $"A new account has been created: [Email:{email};Token:{blankToken};Hash:{token}]");
            
            _logger.Log(LogLevel.Information,
                $"A new account has been created: [Email:{email}]");
            
            return ResultCode.Ok;
        }

        /// <summary>
        ///     Verifies an unverified account from the user token.
        /// </summary>
        /// <param name="token">Unique token of the user.</param>
        public async Task<bool> VerifyAccountAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }
            
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            var user = await db.Users.FirstOrDefaultAsync(x => x.Token == token);
            if (user is null || user.Type != AccountType.Unverified)
            {
                return false;
            }

            user.Type = AccountType.User;
            db.Users.Update(user);
            await db.SaveChangesAsync();
            
            _logger.Log(LogLevel.Information, 
                $"An account has been verified: [Email:{user.Email}]");
            
            return true;
        }
    }
}
