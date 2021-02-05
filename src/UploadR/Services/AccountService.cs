using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UploadR.Database;
using UploadR.Database.Enums;
using UploadR.Database.Models;
using UploadR.Enums;
using UploadR.Models;
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
            
            _logger.LogDebug("User account token reset [Email:{Email};Token:{Token}]", user.Email, user.Token);
            _logger.LogInformation("User account token reset [Email:{Email}]", user.Email);
            
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
            await db.SaveChangesAsync();
            
            _logger.LogInformation("User account state changed [Email:{Email};Blocked:{Disabled}]", user.Email, user.Disabled);

            return ResultCode.Ok;
        }

        /// <summary>
        ///     Deletes an account with the given token.
        /// </summary>
        /// <param name="userGuid">Token of that account.</param>
        public async Task<ResultCode> DeleteAccountAsync(Guid userGuid)
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
                return ResultCode.Unauthorized;
            }

            db.Users.Remove(user);
            await db.SaveChangesAsync();
            
            _logger.LogInformation("User account removed [Email:{Email}]", user.Email);

            return ResultCode.Ok;
        }

        /// <summary>
        ///     Creates a new account with the given email.
        /// </summary>
        /// <param name="model">Model received for the creation of the new account.</param>
        public async Task<ResultCode> CreateAccountAsync(AccountCreateModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                if (!RegexUtilities.IsValidEmail(model.Email))
                {
                    return ResultCode.Invalid;
                }
            }

            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                if (await db.Users.AnyAsync(x => x.Email.ToLower() == model.Email.ToLower()))
                {
                    return ResultCode.EmailInUse;
                }
            }
            else
            {
                model.Email = "anonymous";
            }

            var userId = Guid.NewGuid();
            var token = Guid.NewGuid();

            await db.Users.AddAsync(new User
            {
                Guid = userId,
                CreatedAt = DateTime.Now,
                Disabled = false,
                Email = model.Email,
                Type = AccountType.Unverified,
                Token = token.ToString()
            });

            await db.SaveChangesAsync();

            _logger.LogTrace("A new account has been created: [Id:{UserId};Email:{Email};Token:{Token}]", userId, model.Email, token);
            _logger.LogDebug("A new account has been created: [Id:{UserId};Email:{Email}]", userId, model.Email);
            
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
            
            _logger.LogInformation("An account has been verified: [Id:{Guid};Email:{Email}]", user.Guid, user.Email);
            
            return true;
        }
    }
}
