using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UploadR.Configurations;
using UploadR.Database;
using UploadR.Database.Enums;
using UploadR.Database.Models;
using UploadR.Enums;
using UploadR.Models;

namespace UploadR.Services 
{
    public class ShortenService
    {
        private const string CharsPattern = "abcdefghijklmnopqrstuvwxyz";
        
        private readonly IServiceProvider _services;
        private readonly SHA512Managed _sha512Managed;
        private readonly ILogger<ShortenService> _logger;
        private readonly Random _rng;
        private readonly ShortenConfiguration _shortenConfiguration;

        public ShortenService(
            IServiceProvider services,
            SHA512Managed sha512Managed,
            EntityConfigurationProvider<ShortenConfiguration> shortenConfigurationProvider,
            ILogger<ShortenService> logger)
        {
            _services = services;
            _sha512Managed = sha512Managed;
            _logger = logger;
            _rng = new Random(Guid.NewGuid().GetHashCode());
            _shortenConfiguration = shortenConfigurationProvider.GetConfiguration();
        }

        /// <summary>
        ///     Gets a shortened url.
        /// </summary>
        /// <param name="shortenName">Name of the shortened url.</param>
        /// <param name="password">Password of the shortened url if necessary.</param>
        public async Task<(ResultCode, string)> GetAsync(
            string shortenName,
            string password)
        {
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            var shorten = 
                await db.ShortenedUrls.FirstOrDefaultAsync(x => x.Identifier == shortenName);

            if (shorten is null || shorten.Removed)
            {
                return (ResultCode.NotFound, null);
            }

            if (!string.IsNullOrWhiteSpace(shorten.Password))
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    return (ResultCode.Unauthorized, null);
                }

                var byteHash = _sha512Managed.ComputeHash(Encoding.UTF8.GetBytes(password));
                var passwordHash = string.Join("", byteHash.Select(x => x.ToString("X2")));

                if (shorten.Password != passwordHash)
                {
                    return (ResultCode.Unauthorized, null);
                }
            }
            
            shorten.SeenCount++;
            shorten.LastSeen = DateTime.Now;
            db.ShortenedUrls.Update(shorten);
            await db.SaveChangesAsync();

            return (ResultCode.Ok, shorten.Url);
        }

        /// <summary>
        ///     Gets shortened url details created by a specific user guid.
        /// </summary>
        /// <param name="userGuid">User guid to see the details from.</param>
        /// <param name="limit">Amount of shortened urls to lookup.</param>
        /// <param name="afterGuid">Guid that defines the start of the query.</param>
        public async Task<IReadOnlyList<ShortenDetailsModel>> GetDetailsBulkAsync(
            Guid userGuid,
            int limit,
            Guid? afterGuid)
        {
            if (limit > _shortenConfiguration.BulkLimit)
            {
                return null;
            }
            
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            if (await db.Users.FindAsync(userGuid) is null)
            {
                return null;
            }

            var shortens = db.ShortenedUrls.Where(x => x.AuthorGuid == userGuid && !x.Removed);
            shortens = shortens.OrderBy(x => x.CreatedAt);
            
            var firstShorten = await shortens.FirstOrDefaultAsync(x => x.Guid == afterGuid);
            if (firstShorten is null)
            {
                firstShorten = shortens.FirstOrDefault();
                if (firstShorten is null)
                {
                    return null;
                }
            }
            
            var createdAt = firstShorten.CreatedAt;
            shortens = shortens.Where(x => x.CreatedAt > createdAt);
            shortens = shortens.Take(limit);

            foreach (var shorten in shortens)
            {
                shorten.SeenCount++;
                shorten.LastSeen = DateTime.Now;
            }
            
            db.ShortenedUrls.UpdateRange(shortens);
            await db.SaveChangesAsync();

            return await shortens.Select(shorten => new ShortenDetailsModel
            {
                AuthorGuid = shorten.AuthorGuid,
                CreatedAt = shorten.CreatedAt,
                ExpireAfter = shorten.ExpiryTime,
                HasPassword = string.IsNullOrWhiteSpace(shorten.Password),
                LastSeen = shorten.LastSeen,
                SeenCount = shorten.SeenCount,
                ShortenedGuid = shorten.Guid,
                ShortenedUrl = shorten.Identifier
            }).ToListAsync();
        }
        
        /// <summary>
        ///     Gets the details about a shortened url.
        /// </summary>
        /// <param name="shortenId">Id or name of the shortened url.</param>
        public async Task<ShortenDetailsModel> GetDetailsAsync(
            string shortenId)
        {
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            var shorten = Guid.TryParse(shortenId, out var guid) 
                ? await db.ShortenedUrls.FindAsync(guid) 
                : await db.ShortenedUrls.FirstOrDefaultAsync(x => x.Identifier == shortenId);

            if (shorten is null || shorten.Removed)
            {
                return null;
            }

            shorten.SeenCount++;
            shorten.LastSeen = DateTime.Now;
            db.ShortenedUrls.Update(shorten);
            await db.SaveChangesAsync();

            return new ShortenDetailsModel
            {
                AuthorGuid = shorten.AuthorGuid,
                CreatedAt = shorten.CreatedAt,
                ExpireAfter = shorten.ExpiryTime,
                HasPassword = string.IsNullOrWhiteSpace(shorten.Password),
                LastSeen = shorten.LastSeen,
                SeenCount = shorten.SeenCount,
                ShortenedGuid = shorten.Guid,
                ShortenedUrl = shorten.Identifier
            };
        }

        /// <summary>
        ///     Delete a shortened url.
        /// </summary>
        /// <param name="userGuid">Guid of the user making the request.</param>
        /// <param name="shortenId">Id or name of the shortened url.</param>
        public async Task<ResultCode> DeleteAsync(
            Guid userGuid,
            string shortenId)
        {
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            var shorten = Guid.TryParse(shortenId, out var guid) 
                ? await db.ShortenedUrls.FindAsync(guid) 
                : await db.ShortenedUrls.FirstOrDefaultAsync(x => x.Identifier == shortenId);
            
            if (shorten is null || shorten.Removed)
            {
                return ResultCode.NotFound;
            }

            if (shorten.AuthorGuid != userGuid)
            {
                var currentUser = await db.Users.FindAsync(userGuid);
                if (currentUser.Type != AccountType.Admin)
                {
                    return ResultCode.Unauthorized;
                }
            }

            shorten.Removed = true;
            shorten.LastSeen = DateTime.Now;
            db.ShortenedUrls.Update(shorten);
            await db.SaveChangesAsync();

            _logger.LogInformation(
                "Shortened url deleted by {UserGuid}: [authorguid:{AuthorGuid};guid:{ShortenGuid}]",
                userGuid,
                shorten.AuthorGuid,
                shorten.Guid);
            
            return ResultCode.Ok;
        }

        /// <summary>
        ///     Shorten the given url.
        /// </summary>
        /// <param name="userGuid">Guid of the user making the request.</param>
        /// <param name="url">Url to shorten.</param>
        /// <param name="proposal">Proposed url by the user.</param>
        /// <param name="password">Password to protect that shortened url.</param>
        /// <param name="expireAfter">Expiry time of the shortened url.</param>
        public async Task<ShortenOutModel> ShortenAsync(
            Guid userGuid,
            string url,
            string proposal,
            string password,
            TimeSpan expireAfter)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }
        
            if (expireAfter == TimeSpan.Zero)
            {
                expireAfter = _shortenConfiguration.DefaultExpiry;
            }
            
            var model = new ShortenOutModel
            {
                BaseUrl = url,
                ExpireAfter = expireAfter,
                HasPassword = !string.IsNullOrWhiteSpace(password),
                ShortenedUrl = proposal
            };
            
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();
            
            if (string.IsNullOrWhiteSpace(model.ShortenedUrl) 
                || model.ShortenedUrl.Length > _shortenConfiguration.SizeMax
                || model.ShortenedUrl.Length < _shortenConfiguration.SizeMin)
            {
                model.ShortenedUrl = GetRandomName(_shortenConfiguration.DefaultSize);
            }
            
            var all = await db.ShortenedUrls.ToListAsync();
            while (all.Any(x => x.Identifier == model.ShortenedUrl))
            {
                model.ShortenedUrl = GetRandomName(_shortenConfiguration.DefaultSize);
            }
            
            var passwordHash = "";
            if (!string.IsNullOrWhiteSpace(password))
            {
                var byteHash = _sha512Managed.ComputeHash(
                    Encoding.UTF8.GetBytes(password));
                passwordHash = string.Join("", 
                    byteHash.Select(x => x.ToString("X2")));
            }

            var entry = await db.ShortenedUrls.AddAsync(new ShortenedUrl
            {
                Guid = Guid.NewGuid(),
                Password = passwordHash,
                Removed = false,
                Identifier = model.ShortenedUrl,
                Url = model.BaseUrl,
                CreatedAt = DateTime.Now,
                AuthorGuid = userGuid,
                ExpiryTime = expireAfter,
                SeenCount = 0,
                LastSeen = DateTime.Now
            });

            await db.SaveChangesAsync();
            
            var ecs = _services.GetRequiredService<ExpiryCheckService<ShortenedUrl>>();
            await ecs.RestartAsync();

            _logger.LogInformation(
                "Shorten by {UserGuid}: [url:{Url};shorten:{Shorten};haspassword:{HasPassword};expire_in_ms:{Expiry}]",
                userGuid,
                entry.Entity.Url,
                entry.Entity.Identifier,
                !string.IsNullOrWhiteSpace(entry.Entity.Password),
                entry.Entity.ExpiryTime.TotalMilliseconds);
            
            return model;
        }

        /// <summary>
        ///     Generates a random sized name for a shortened url.
        /// </summary>
        /// <param name="size">Size of the random string to generate.</param>
        public string GetRandomName(int size)
        {
            return string.Create(size, _rng, (span, random) =>
            {
                for (var i = 0; i < span.Length; ++i)
                {
                    span[i] = CharsPattern[random.Next(CharsPattern.Length)];
                }
            });
        }
    }
}
