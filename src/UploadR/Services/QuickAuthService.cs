using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UploadR.Configurations;
using UploadR.Database;
using UploadR.Database.Models;
using UploadR.Interfaces;

namespace UploadR.Services
{
    public sealed class QuickAuthService
    {
        private readonly IServiceProvider _sp;
        private readonly OneTimeTokenConfiguration _ottConfiguration;
        private readonly RateLimiterService<QuickAuthService> _rl;

        private readonly ConcurrentDictionary<Guid, OneTimeToken> _oneTimeTokens;

        public QuickAuthService(IServiceProvider sp, IOneTimeTokenConfigurationProvider ottConfiguration, RateLimiterService<QuickAuthService> rl)
        {
            _sp = sp;
            _ottConfiguration = ottConfiguration.GetConfiguration();
            _rl = rl;

            _oneTimeTokens = new ConcurrentDictionary<Guid, OneTimeToken>();
        }

        public bool IncrementAndValidateRateLimits(int hashIp)
        {
            if (!_ottConfiguration.AntiSpam.Enabled)
            {
                return true;
            }

            var time = TimeSpan.FromMinutes(_ottConfiguration.AntiSpam.Timeout);
            var maxTries = _ottConfiguration.AntiSpam.MaxTry;

            return _rl.IncrementAndValidateRateLimits(hashIp, time, maxTries);
        }

        /// <summary>
        ///     Gets or create a new <see cref="OneTimeToken"/> for the specified user.
        /// </summary>
        /// <param name="userGuid">User that will be logged in after validating the <see cref="OneTimeToken"/></param>
        public OneTimeToken GetOrCreate(Guid userGuid)
        {
            using var db = _sp.GetRequiredService<UploadRContext>();
            return GetOrCreate(db.Users.Find(userGuid));
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

        public int GetRemainingTimeout(int hashIp)
        {
            return _rl.GetRemainingTimeout(hashIp);
        }

        /// <summary>
        ///     Returns the User depending on the <see cref="OneTimeToken"/>.
        /// </summary>
        /// <param name="ottGuid"><see cref="Guid"/> of the <see cref="OneTimeToken"/>.</param>
        public User GetAndValidateUserOtt(Guid ottGuid)
        {
            using var db = _sp.GetRequiredService<UploadRContext>();

            if (!_oneTimeTokens.Values.Any(x => x.Token == ottGuid))
            {
                throw new InvalidOperationException("The given one-time-token isn't valid.");
            }

            var ott = _oneTimeTokens.Values.FirstOrDefault(x => x.Token == ottGuid);
            if (ott.IsUsed || ott.ExpiresAt < DateTimeOffset.Now)
            {
                throw new InvalidOperationException("The given one-time-token has expired or has already been used.");
            }

            var user = db.Users.Include(x => x.Token).FirstOrDefault(x => x.Guid == ott.UserGuid);
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
            using var db = _sp.GetRequiredService<UploadRContext>();
            Invalidate(db.Users.Find(userGuid));
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
}
