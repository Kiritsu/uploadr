using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UploadR.Configurations;
using UploadR.Database;
using UploadR.Database.Enums;
using UploadR.Database.Models;
using UploadR.Enum;
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

        public async Task<ServiceResult<Token>> ResetOrRevokeTokenAsync(Guid guid, bool reset)
        {
            await using var db = _sp.GetRequiredService<UploadRContext>();
            var currentToken = await db.Tokens.Include(x => x.User).FirstOrDefaultAsync(x => x.UserGuid == guid);

            if (reset)
            {
                db.Remove(currentToken);

                currentToken = new Token
                {
                    CreatedAt = DateTime.Now,
                    Guid = Guid.NewGuid(),
                    UserGuid = guid,
                    TokenType = currentToken.TokenType,
                    Revoked = false
                };

                await db.AddAsync(currentToken);
            }
            else
            {
                currentToken.Revoked = true;
                db.Update(currentToken);
            }

            await db.SaveChangesAsync();

            return ServiceResult<Token>.Success(currentToken);
        }

        public async Task<ServiceResult<User>> BlockOrUnblockUserAsync(string guidStr, bool block)
        {
            if (!Guid.TryParse(guidStr, out var guid))
            {
                return ServiceResult<User>.Fail(ResultErrorType.InvalidGuid);
            }

            await using var db = _sp.GetRequiredService<UploadRContext>();
            var user = await db.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Guid == guid);

            if (user is null)
            {
                return ServiceResult<User>.Fail(ResultErrorType.Null);
            }

            if (user.Token.TokenType == TokenType.Admin)
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
            var user = await db.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Guid == guid);

            db.Remove(user);
            await db.SaveChangesAsync();
        }

        public async Task<ServiceResult<(User, Token)>> CreateAccountAsync(UserCreateModel model)
        {
            if (!_rc.UserRegisterRoute)
            {
                return ServiceResult<(User, Token)>.Fail(ResultErrorType.Unauthorized);
            }

            var email = "";
            if (!(model is null))
            {
                email = model.Email;
            }

            await using var db = _sp.GetRequiredService<UploadRContext>();
            if (!string.IsNullOrWhiteSpace(email)
                && await db.Users.AnyAsync(x =>
                    x.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                return ServiceResult<(User, Token)>.Fail(ResultErrorType.Found);
            }

            var user = new User
            {
                Guid = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Email = email,
                Disabled = false
            };

            var token = new Token
            {
                Guid = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                UserGuid = user.Guid,
                TokenType = TokenType.User,
                Revoked = false
            };

            await db.AddAsync(user);
            await db.AddAsync(token);
            await db.SaveChangesAsync();

            return ServiceResult<(User, Token)>.Success((user, token));
        }
    }
}
