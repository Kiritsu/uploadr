using System;
using System.Linq;
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
        private readonly ILogger<AccountService> _logger;

        public AccountService(IServiceProvider services, ILogger<AccountService> logger)
        {
            _services = services;
            _logger = logger;
        }

        /// <summary>
        ///     Resets the token of the given user id.
        /// </summary>
        /// <param name="userGuid">Id of the user.</param>
        public async Task<ResultCode> ResetUserTokenAsync(Guid userGuid)
        {
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            var user = await db.Users.FindAsync(userGuid);
            if (user is null)
            {
                return ResultCode.NotFound;
            }

            user.Token = Guid.NewGuid().ToString();
            db.Users.Update(user);
            
            await db.SaveChangesAsync();
            
            _logger.LogDebug("User account token reset [Email:{email};Token:{token}]", user.Email, user.Token);
            
            _logger.LogInformation("User account token reset [Email:{email}]", user.Email);
            
            return ResultCode.Ok;
        }

        /// <summary>
        ///     Blocks an account with the given user id.
        /// </summary>
        /// <param name="userGuid">Id of the user.</param>
        /// <param name="block">Whether to block or unblock the user.</param>
        public async Task<ResultCode> ToggleAccountStateAsync(Guid userGuid, bool block)
        {
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();
            
            var user = await db.Users.FindAsync(userGuid);
            if (user is null)
            {
                return ResultCode.NotFound;
            }

            if (user.Type == AccountType.Admin)
            {
                return ResultCode.Invalid;
            }
            
            user.Disabled = block;
            db.Users.Update(user);
            
            _logger.LogInformation("User account state changed [Email:{email};Blocked:{disabled}]", user.Email, user.Disabled);
            
            await db.SaveChangesAsync();
            return ResultCode.Ok;
        }

        /// <summary>
        ///     Deletes an account with the given token.
        /// </summary>
        /// <param name="userGuid">Token of that account.</param>
        /// <param name="cascade">Whether to delete or not the uploads of that account.</param>
        public async Task<ResultCode> DeleteAccountAsync(Guid userGuid, bool cascade = false)
        {
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();
            
            var user = await db.Users.FindAsync(userGuid);
            if (user is null)
            {
                return ResultCode.NotFound;
            }

            db.Users.Remove(user);

            if (cascade)
            {
                var uploads = db.Uploads.Where(x => x.AuthorGuid == user.Guid);
                db.Uploads.RemoveRange(uploads);
            }

            await db.SaveChangesAsync();
            
            _logger.LogInformation("User account removed [Email:{email};DeleteUploads:{cascade}]", user.Email, cascade);

            return ResultCode.Ok;
        }

        /// <summary>
        ///     Creates a new account with the given email.
        /// </summary>
        /// <param name="email">Email associated with the new account.</param>
        public async Task<ResultCode> CreateAccountAsync(string email)
        {
            if (!RegexUtilities.IsValidEmail(email))
            {
                return ResultCode.Invalid;
            }

            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            if (await db.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower()))
            {
                return ResultCode.EmailInUse;
            }

            var userId = Guid.NewGuid();
            var token = Guid.NewGuid();

            await db.Users.AddAsync(new User
            {
                Guid = userId,
                CreatedAt = DateTime.Now,
                Disabled = false,
                Email = email,
                Type = AccountType.Unverified,
                Token = token.ToString()
            });

            await db.SaveChangesAsync();

            _logger.LogDebug("A new account has been created: [Id:{userId};Email:{email};Token:{token}]", userId, email, token);
            
            _logger.LogDebug("A new account has been created: [Id:{userId};Email:{email}]", userId, email);
            
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
            
            _logger.LogInformation("An account has been verified: [Id:{guid};Email:{email}]", user.Guid, user.Email);
            
            return true;
        }
    }
}
