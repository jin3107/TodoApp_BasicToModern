using StackExchange.Redis;
using Todo.Services.Implementations;
using Todo.Services.Interfaces;

namespace Todo.API.Extensions
{
    public static class RedisServiceExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnection = configuration.GetConnectionString("RedisConnection");
            var redisEnabled = configuration.GetValue<bool>("RedisSettings:Enabled");
            if (redisEnabled && !string.IsNullOrEmpty(redisConnection))
            {
                services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
                {
                    var config = ConfigurationOptions.Parse(redisConnection);
                    config.AbortOnConnectFail = false;
                    config.ConnectTimeout = 5000;
                    config.SyncTimeout = 5000;
                    config.AsyncTimeout = 5000;
                    config.ConnectRetry = 3;
                    config.KeepAlive = 60;
                    // Tăng connection pool để tránh timeout
                    config.DefaultDatabase = 0;
                    return ConnectionMultiplexer.Connect(config);
                });

                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "TodoApp:";
                });

                services.AddScoped<ICacheService, RedisCacheService>();
            } 
            else
            {
                services.AddDistributedMemoryCache();
                services.AddScoped<ICacheService, MemoryCacheService>();
            }

            return services;
        }
    }
}
