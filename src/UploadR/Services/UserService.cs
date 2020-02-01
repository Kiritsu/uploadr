using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UploadR.Configurations;
using UploadR.Database;
using UploadR.Database.Enums;
using UploadR.Database.Models;
using UploadR.Enum;
using UploadR.Extensions;
using UploadR.Interfaces;
using UploadR.Models;

namespace UploadR.Services
{
    public sealed class UserService
    {
        private readonly IServiceProvider _sp;
        private readonly RoutesConfiguration _rc;

        public UserService(IServiceProvider sp, IRoutesConfigurationProvider rc)
        {
            _sp = sp;
            _rc = rc.GetConfiguration();
        }

        public async Task<ServiceResult<User>> ResetOrRevokeTokenAsync(Guid guid, string token, bool reset)
        {
            await using var db = _sp.GetRequiredService<UploadRContext>();
            var user = await db.Users.FindAsync(guid);

            user.Token = null;
            if (reset)
            {
                user.Token = Guid.NewGuid().ToString();
            }

            db.Users.Update(user);
            await db.SaveChangesAsync();

            return ServiceResult<User>.Success(user);
        }

        public async Task<ServiceResult<User>> BlockOrUnblockUserAsync(string guidStr, bool block)
        {
            if (!Guid.TryParse(guidStr, out var guid))
            {
                return ServiceResult<User>.Fail(ResultErrorType.InvalidGuid);
            }

            await using var db = _sp.GetRequiredService<UploadRContext>();
            var user = await db.Users.FindAsync(guid);

            if (user is null)
            {
                return ServiceResult<User>.Fail(ResultErrorType.Null);
            }

            if (user.Type == AccountType.Admin)
            {
                return ServiceResult<User>.Fail(ResultErrorType.Unauthorized);
            }

            user.Disabled = block;
            db.Update(user);
            await db.SaveChangesAsync();

            return ServiceResult<User>.Success(user);
        }

        public async Task DeleteAccountAsync(Guid guid)
        {
            await using var db = _sp.GetRequiredService<UploadRContext>();
            var user = await db.Users.FindAsync(guid);

            db.Remove(user);
            await db.SaveChangesAsync();
        }

        public async Task<ServiceResult<User>> CreateAccountAsync(UserCreateModel model)
        {
            if (!_rc.UserRegisterRoute)
            {
                return ServiceResult<User>.Fail(ResultErrorType.Unauthorized);
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return ServiceResult<User>.Fail(ResultErrorType.EmailNotProvided);
            }

            if (!model.Email.IsValidEmail())
            {
                return ServiceResult<User>.Fail(ResultErrorType.EmailNotProvided);
            }

            await using var db = _sp.GetRequiredService<UploadRContext>();
            if (await db.Users.AnyAsync(x => x.Email.ToLower() == model.Email.ToLower()))
            {
                return ServiceResult<User>.Fail(ResultErrorType.Found);
            }

            var user = new User
            {
                Guid = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Email = model.Email,
                Disabled = false,
                Type = AccountType.User,
                Token = Guid.NewGuid().ToString()
            };

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            return ServiceResult<User>.Success(user);
        }
    }
}
