using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShareY.Configurations;
using ShareY.Database;
using ShareY.Database.Models;
using ShareY.Interfaces;

namespace ShareY.Services
{
    public sealed class QuickAuthService
    {
        private readonly ShareYContext _dbContext;
        private readonly OneTimeTokenConfiguration _ottConfiguration;

        private readonly ConcurrentDictionary<Guid, OneTimeToken> _oneTimeTokens;
        private readonly ConcurrentDictionary<int, (DateTimeOffset, int)> _rateLimitHitPerIpHash;

        public QuickAuthService(ShareYContext dbContext, IOneTimeTokenConfigurationProvider ottConfiguration)
        {
            _dbContext = dbContext;
            _ottConfiguration = ottConfiguration.GetConfiguration();

            _oneTimeTokens = new ConcurrentDictionary<Guid, OneTimeToken>();
            _rateLimitHitPerIpHash = new ConcurrentDictionary<int, (DateTimeOffset, int)>();
        }

        /// <summary>
        ///     Increments and checks if the user's ip is being rate limited.
        /// </summary>
        /// <param name="hashIp">Hash of the user's ip.</param>
        public bool IncrementAndValidateRateLimits(int hashIp)
        {
            if (!_rateLimitHitPerIpHash.TryGetValue(hashIp, out var rate))
            {
                _rateLimitHitPerIpHash.TryAdd(hashIp, (DateTimeOffset.Now + TimeSpan.FromMinutes(_ottConfiguration.AntiSpam.Timeout), 1));
            }
            else
            {
                if (rate.Item1 - DateTimeOffset.Now <= TimeSpan.Zero)
                {
                    _rateLimitHitPerIpHash[hashIp] = (DateTimeOffset.Now + TimeSpan.FromMinutes(_ottConfiguration.AntiSpam.Timeout), 1);
                }
                else if (_rateLimitHitPerIpHash[hashIp].Item2 > _ottConfiguration.AntiSpam.MaxTry)
                {
                    return false;
                }
                else
                {
                    _rateLimitHitPerIpHash[hashIp] = (DateTimeOffset.Now + TimeSpan.FromMinutes(_ottConfiguration.AntiSpam.Timeout), rate.Item2 + 1);
                }
            }

            return true;
        }

        /// <summary>
        ///     Gets or create a new <see cref="OneTimeToken"/> for the specified user.
        /// </summary>
        /// <param name="userGuid">User that will be logged in after validating the <see cref="OneTimeToken"/></param>
        public OneTimeToken GetOrCreate(Guid userGuid, int hashIp)
        {
            return GetOrCreate(_dbContext.Users.Find(userGuid), hashIp);
        }

        /// <summary>
        ///     Gets or create a new <see cref="OneTimeToken"/> for the specified user.
        /// </summary>
        /// <param name="user">User that will be logged in after validating the <see cref="OneTimeToken"/></param>
        public OneTimeToken GetOrCreate(User user, int hashIp)
        {
            if (user == null)
            {
                throw new NullReferenceException("Provided user is null.");
            }

            if (!IncrementAndValidateRateLimits(hashIp))
            {
                throw new InvalidOperationException($"You are being rate limited. Retry in {Math.Round((_rateLimitHitPerIpHash[hashIp].Item1 - DateTimeOffset.Now).TotalMinutes)} minutes.");
            }

            if (!_oneTimeTokens.TryGetValue(user.Guid, out var ott))
            {
                ott = OneTimeToken.New(user.Guid, TimeSpan.FromMinutes(_ottConfiguration.Timeout));
                _oneTimeTokens.TryAdd(user.Guid, ott);
            }

            if (ott.IsUsed || ott.ExpiresAt < DateTimeOffset.Now)
            {
                ott = OneTimeToken.New(user.Guid, TimeSpan.FromMinutes(_ottConfiguration.Timeout));
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
