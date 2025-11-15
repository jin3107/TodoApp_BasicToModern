using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Todo.Services.Interfaces;

namespace Todo.Services.Implementations
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private ILogger<RedisCacheService> _logger;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(10);

        public RedisCacheService(IDistributedCache distributedCache,
            IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger)
        {
            _distributedCache = distributedCache;
            _connectionMultiplexer = connectionMultiplexer;
            _logger = logger;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var data = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(data))
                    return false;

                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis ExistsAsync failed for key: {Key}", key);
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
                _logger.LogError(ex, "Redis GetAsync failed for key: {Key}", key);
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
                _logger.LogError(ex, "Redis RemoveAsync failed for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: pattern).ToArray
                    ();
                if (keys.Length > 0)
                {
                    var database = _connectionMultiplexer.GetDatabase();
                    await database.KeyDeleteAsync(keys);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis RemoveByPatternAsync failed for pattern: {Pattern}", pattern);
            }
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
                _logger.LogError(ex, "Redis SetAsync failed for key: {Key}", key);
            }
        }
    }
}
