using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Droniverse.Shared.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;

        public CacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var rawValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(rawValue);
        }

        public async Task SetAsync<T>(
            string key,
            T value,
            int absoluteExpirationSeconds,
            int? slidingExpirationSeconds = null,
            CancellationToken cancellationToken = default)
        {
            var serializedValue = JsonSerializer.Serialize(value);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(absoluteExpirationSeconds),
                SlidingExpiration = slidingExpirationSeconds.HasValue
                    ? TimeSpan.FromSeconds(slidingExpirationSeconds.Value)
                    : null
            };

            await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return _distributedCache.RemoveAsync(key, cancellationToken);
        }
    }
}
