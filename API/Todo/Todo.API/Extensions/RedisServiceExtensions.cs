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
                    var configuration = ConfigurationOptions.Parse(redisConnection);
                    configuration.AbortOnConnectFail = false;
                    return ConnectionMultiplexer.Connect(configuration);
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
