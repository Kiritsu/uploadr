using System;
using System.Collections.Concurrent;

namespace UploadR.Services
{
    public sealed class RateLimiterService<T>
    {
        private readonly ConcurrentDictionary<int, (DateTimeOffset, int)> _rateLimitHitPerIpHash;

        public RateLimiterService()
        {
            _rateLimitHitPerIpHash = new ConcurrentDictionary<int, (DateTimeOffset, int)>();
        }

        public int GetRemainingTimeout(int hashIp)
        {
            return (int)Math.Round((_rateLimitHitPerIpHash[hashIp].Item1 - DateTimeOffset.Now).TotalMinutes) + 1;
        }

        /// <summary>
        ///     Increments and checks if the user's ip is being rate limited.
        /// </summary>
        /// <param name="hashIp">Hash of the user's ip.</param>
        public bool IncrementAndValidateRateLimits(int hashIp, TimeSpan timeout, long maxTries)
        {
            if (!_rateLimitHitPerIpHash.TryGetValue(hashIp, out var rate))
            {
                _rateLimitHitPerIpHash.TryAdd(hashIp, (DateTimeOffset.Now + timeout, 1));
            }
            else
            {
                if (rate.Item1 - DateTimeOffset.Now <= TimeSpan.Zero)
                {
                    _rateLimitHitPerIpHash[hashIp] = (DateTimeOffset.Now + timeout, 1);
                }
                else if (_rateLimitHitPerIpHash[hashIp].Item2 > maxTries)
                {
                    return false;
                }
                else
                {
                    _rateLimitHitPerIpHash[hashIp] = (DateTimeOffset.Now + timeout, rate.Item2 + 1);
                }
            }

            return true;
        }
    }
}
