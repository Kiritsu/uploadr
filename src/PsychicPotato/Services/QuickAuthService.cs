﻿using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PsychicPotato.Configurations;
using PsychicPotato.Database;
using PsychicPotato.Database.Models;
using PsychicPotato.Interfaces;

namespace PsychicPotato.Services
{
    public sealed class QuickAuthService
    {
        private readonly PsychicPotatoContext _dbContext;
        private readonly OneTimeTokenConfiguration _ottConfiguration;

        private readonly ConcurrentDictionary<Guid, OneTimeToken> _oneTimeTokens;
        private readonly ConcurrentDictionary<int, (DateTimeOffset, int)> _rateLimitHitPerIpHash;

        public ReadOnlyDictionary<int, (DateTimeOffset, int)> RateLimitHitPerIpHash;

        public QuickAuthService(PsychicPotatoContext dbContext, IOneTimeTokenConfigurationProvider ottConfiguration)
        {
            _dbContext = dbContext;
            _ottConfiguration = ottConfiguration.GetConfiguration();

            _oneTimeTokens = new ConcurrentDictionary<Guid, OneTimeToken>();
            _rateLimitHitPerIpHash = new ConcurrentDictionary<int, (DateTimeOffset, int)>();
            RateLimitHitPerIpHash = new ReadOnlyDictionary<int, (DateTimeOffset, int)>(_rateLimitHitPerIpHash);
        }

        /// <summary>
        ///     Increments and checks if the user's ip is being rate limited.
        /// </summary>
        /// <param name="hashIp">Hash of the user's ip.</param>
        public bool IncrementAndValidateRateLimits(int hashIp)
        {
            if (!_ottConfiguration.AntiSpam.Enabled)
            {
                return true;
            }

            var mins = TimeSpan.FromMinutes(_ottConfiguration.AntiSpam.Timeout);

            if (!_rateLimitHitPerIpHash.TryGetValue(hashIp, out var rate))
            {
                _rateLimitHitPerIpHash.TryAdd(hashIp, (DateTimeOffset.Now + mins, 1));
            }
            else
            {
                if (rate.Item1 - DateTimeOffset.Now <= TimeSpan.Zero)
                {
                    _rateLimitHitPerIpHash[hashIp] = (DateTimeOffset.Now + mins, 1);
                }
                else if (_rateLimitHitPerIpHash[hashIp].Item2 > _ottConfiguration.AntiSpam.MaxTry)
                {
                    return false;
                }
                else
                {
                    _rateLimitHitPerIpHash[hashIp] = (DateTimeOffset.Now + mins, rate.Item2 + 1);
                }
            }

            return true;
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
