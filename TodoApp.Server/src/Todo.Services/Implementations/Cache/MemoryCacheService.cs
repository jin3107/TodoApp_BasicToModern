using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Todo.Services.Interfaces;

namespace Todo.Services.Implementations
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(10);

        public MemoryCacheService(IDistributedCache distributedCache, ILogger<MemoryCacheService> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var data = await _distributedCache.GetStringAsync(key);
                return !string.IsNullOrEmpty(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Memory Cache ExistsAsync failed for key: {Key}", key);
                return false;
            }
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cachedData = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("Cache miss for key: {Key}", key);
                    return null;
                }

                return JsonSerializer.Deserialize<T>(cachedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Memory Cache GetAsync failed for key: {Key}", key);
                return null;
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Memory Cache RemoveAsync failed for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            _logger.LogWarning("RemoveByPatternAsync is not supported in Memory Cache. Pattern: {Pattern}", pattern);
            await Task.CompletedTask;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var serializedData = JsonSerializer.Serialize(value);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration,
                };
                await _distributedCache.SetStringAsync(key, serializedData, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Memory Cache SetAsync failed for key: {Key}", key);
            }
        }
    }
}
