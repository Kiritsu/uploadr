using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShareY.Database;
using ShareY.Database.Models;

namespace ShareY.Services
{
    public sealed class QuickAuthService
    {
        private readonly ShareYContext _dbContext;

        private readonly ConcurrentDictionary<Guid, OneTimeToken> _oneTimeTokens;

        public QuickAuthService(ShareYContext dbContext)
        {
            _dbContext = dbContext;

            _oneTimeTokens = new ConcurrentDictionary<Guid, OneTimeToken>();
        }

        /// <summary>
        ///     Gets or create a new <see cref="OneTimeToken"/> for the specified user.
        /// </summary>
        /// <param name="userGuid">User that will be logged in after validating the <see cref="OneTimeToken"/></param>
        public OneTimeToken GetOrCreate(Guid userGuid)
        {
            return GetOrCreate(_dbContext.Users.Find(userGuid));
        }

        /// <summary>
        ///     Gets or create a new <see cref="OneTimeToken"/> for the specified user.
        /// </summary>
        /// <param name="user">User that will be logged in after validating the <see cref="OneTimeToken"/></param>
        public OneTimeToken GetOrCreate(User user)
        {
            if (user == null)
            {
                throw new NullReferenceException("Provided user is null.");
            }

            if (!_oneTimeTokens.TryGetValue(user.Guid, out var ott))
            {
                ott = OneTimeToken.New(user.Guid);
                _oneTimeTokens.TryAdd(user.Guid, ott);
            }

            if (ott.IsUsed || ott.ExpiresAt < DateTimeOffset.Now)
            {
                ott = OneTimeToken.New(user.Guid);
                _oneTimeTokens[user.Guid] = ott;
            }

            return ott;
        }

        /// <summary>
        ///     Returns the User depending on the <see cref="OneTimeToken"/>.
        /// </summary>
        /// <param name="ottGuid"><see cref="Guid"/> of the <see cref="OneTimeToken"/>.</param>
        public User GetAndValidateUserOtt(Guid ottGuid)
        {
            if (!_oneTimeTokens.Values.Any(x => x.Token == ottGuid))
            {
                throw new InvalidOperationException("The given one-time-token isn't valid.");
            }

            var ott = _oneTimeTokens.Values.FirstOrDefault(x => x.Token == ottGuid);
            if (ott.IsUsed || ott.ExpiresAt < DateTimeOffset.Now)
            {
                throw new InvalidOperationException("The given one-time-token has expired or has already been used.");
            }

            var user = _dbContext.Users.Include(x => x.Token).FirstOrDefault(x => x.Guid == ott.UserGuid);
            if (user == null)
            {
                throw new InvalidOperationException("The target user is not valid. This should not happen.");
            }

            return user;
        }

        /// <summary>
        ///     Invalidates the <see cref="OneTimeToken"/> for the specified <see cref="User"/>.
        /// </summary>
        /// <param name="userGuid"><see cref="User"/> which will have its <see cref="OneTimeToken"/> invalidated.</param>
        public void Invalidate(Guid userGuid)
        {
            Invalidate(_dbContext.Users.Find(userGuid));
        }

        /// <summary>
        ///     Invalidates the <see cref="OneTimeToken"/> for the specified <see cref="User"/>.
        /// </summary>
        /// <param name="user"><see cref="User"/> which will have its <see cref="OneTimeToken"/> invalidated.</param>
        public void Invalidate(User user)
        {
            if (!_oneTimeTokens.TryGetValue(user.Guid, out var ott))
            {
                return;
            }

            ott.IsUsed = true;
        }
    }

    public class OneTimeToken
    {
        public Guid UserGuid;
        public Guid Token;
        public bool IsUsed;
        public DateTimeOffset ExpiresAt;

        public static OneTimeToken New(Guid userGuid, TimeSpan durationTime = default)
        {
            if (durationTime == default)
            {
                durationTime = TimeSpan.FromMinutes(30);
            }

            var ott = new OneTimeToken
            {
                UserGuid = userGuid,
                Token = Guid.NewGuid(),
                ExpiresAt = DateTimeOffset.Now.Add(durationTime),
                IsUsed = false
            };

            return ott;
        }
    }
}
