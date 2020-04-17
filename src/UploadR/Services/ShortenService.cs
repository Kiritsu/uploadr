using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using UploadR.Database;
using UploadR.Database.Models;
using UploadR.Models;

namespace UploadR.Services 
{
    public class ShortenService
    {
        private const string CharsPattern = "abcdefghijklmnopqrstuvwxyz";
        
        private readonly IServiceProvider _services;
        private readonly Random _rng;

        public ShortenService(IServiceProvider services)
        {
            _services = services;
            _rng = new Random(Guid.NewGuid().GetHashCode());
        }
        
        public async Task<ShortenOutModel> ShortenAsync(
            Guid userGuid,
            string url,
            string proposal,
            string password,
            TimeSpan expireAfter)
        {
            var model = new ShortenOutModel
            {
                BaseUrl = url,
                ExpireAfter = expireAfter,
                HasPassword = !string.IsNullOrWhiteSpace(password),
                ShortenedUrl = proposal
            };
            
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();
            
            if (string.IsNullOrWhiteSpace(model.ShortenedUrl))
            {
                model.ShortenedUrl = GetRandomName(8);
            }
            
            var all = await db.ShortenedUrls.ToListAsync();
            while (all.Any(x => x.Shorten == model.ShortenedUrl))
            {
                model.ShortenedUrl = GetRandomName(8);
            }

            await db.ShortenedUrls.AddAsync(new ShortenedUrl
            {
                Guid = Guid.NewGuid(),
                Password = password,
                Removed = false,
                Shorten = model.ShortenedUrl,
                Url = model.BaseUrl,
                CreatedAt = DateTime.Now,
                AuthorGuid = userGuid,
                ExpiryTime = expireAfter,
                SeenCount = 0,
                LastSeen = DateTime.Now
            });

            await db.SaveChangesAsync();
            return model;
        }

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
