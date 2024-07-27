using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using SpaceWeatherStation.Interfaces;
using System.Text.Json;

namespace SpaceWeatherStation.Cache
{
    public class CacheManager: ICacheManager
    {
        private readonly IDistributedCache _distributedCache;

        public CacheManager(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task SetCacheAsync<T>(string key, T value, TimeSpan expirationTime)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime
            };

            var jsonData = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, jsonData, options);
        }

        public async Task<T> GetCacheAsync<T>(string key)
        {
            var jsonData = await _distributedCache.GetStringAsync(key);
            return jsonData is not null ? JsonSerializer.Deserialize<T>(jsonData) : default;
        }

        public async Task RemoveCacheAsync(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }
    }
}
