using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using Todo.Application.Interfaces.Cache;

namespace Todo.Infrastructure.Implementations.Cache
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
                return !string.IsNullOrEmpty(data);
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
                var database = _connectionMultiplexer.GetDatabase();
                var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());

                var fullPattern = $"TodoApp:{pattern}";

                var keysToDelete = new List<RedisKey>();
                await foreach (var key in server.KeysAsync(pattern: fullPattern, pageSize: 250))
                {
                    keysToDelete.Add(key);

                    if (keysToDelete.Count >= 100)
                    {
                        await database.KeyDeleteAsync(keysToDelete.ToArray());
                        _logger.LogInformation("Deleted batch of {Count} keys", keysToDelete.Count);
                        keysToDelete.Clear();
                    }
                }

                if (keysToDelete.Count > 0)
                {
                    await database.KeyDeleteAsync(keysToDelete.ToArray());
                    _logger.LogInformation("Deleted final batch of {Count} keys for pattern: {Pattern}", keysToDelete.Count, fullPattern);
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
