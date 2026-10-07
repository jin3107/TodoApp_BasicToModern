using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using StackExchange.Redis;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.Authentication;
using Todo.Application.Interfaces.Background;
using Todo.Application.Interfaces.Cache;
using Todo.Application.Interfaces.Repositories;
using Todo.Infrastructure.Implementations;
using Todo.Infrastructure.Implementations.Authentication;
using Todo.Infrastructure.Implementations.Background;
using Todo.Infrastructure.Implementations.Cache;
using Todo.Infrastructure.Jobs;
using Todo.Infrastructure.Persistence.Repositories;

namespace Todo.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            var redisConn = configuration.GetConnectionString("RedisConnection");
            var redisEnabled = configuration.GetValue<bool>("RedisSettings:Enabled");
            if (redisEnabled && !string.IsNullOrEmpty(redisConn))
            {
                services.AddSingleton<IConnectionMultiplexer>(_ =>
                {
                    var opts = ConfigurationOptions.Parse(redisConn);
                    opts.AbortOnConnectFail = false;
                    opts.ConnectTimeout = 5000;
                    opts.SyncTimeout = 5000;
                    opts.AsyncTimeout = 5000;
                    opts.ConnectRetry = 3;
                    opts.KeepAlive = 60;
                    opts.DefaultDatabase = 0;
                    return ConnectionMultiplexer.Connect(opts);
                });
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConn;
                    options.InstanceName = "TodoApp:";
                });
                services.AddScoped<ICacheService, RedisCacheService>();
            }
            else
            {
                services.AddDistributedMemoryCache();
                services.AddScoped<ICacheService, MemoryCacheService>();
            }

            services.AddScoped<ITodoItemRepository, TodoItemRepository>();
            services.AddScoped<ITodoListRepository, TodoListRepository>();
            services.AddScoped<ITodoItemProgressReportRepository, TodoItemProgressReportRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IBlacklistedTokenRepository, BlacklistedTokenRepository>();
            services.AddScoped<IOtpCodeRepository, OtpCodeRepository>();

            services.AddScoped<IEmailService, EmailService>();

            services.AddScoped<IUserService, UserService>();

            services.AddScoped<ILoginHandler, LoginHandler>();
            services.AddScoped<IRegisterHandler, RegisterHandler>();
            services.AddScoped<IRefreshTokenHandler, RefreshTokenHandler>();
            services.AddScoped<ILogoutHandler, LogoutHandler>();
            services.AddScoped<IChangePasswordHandler, ChangePasswordHandler>();
            services.AddScoped<ISendOtpHandler, SendOtpHandler>();
            services.AddScoped<IVerifyOtpHandler, VerifyOtpHandler>();

            services.AddQuartz(q =>
            {
                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 3);

                var dailyKey = new JobKey("DailyTaskReportJob", "EmailJobs");
                q.AddJob<DailyTodoItemReportJob>(opts => opts
                    .WithIdentity(dailyKey)
                    .WithDescription("Send daily task report email at 6:00 PM"));
                q.AddTrigger(opts => opts
                    .ForJob(dailyKey)
                    .WithIdentity("DailyReportTrigger", "EmailJobs")
                    .WithCronSchedule("0 0 18 * * ?")
                    .WithDescription("Trigger for daily report at 6:00 PM"));

                var weeklyKey = new JobKey("WeeklyTaskSummaryJob", "EmailJobs");
                q.AddJob<WeeklyTaskSummaryJob>(opts => opts
                    .WithIdentity(weeklyKey)
                    .WithDescription("Send weekly task summary every Monday at 9:00 AM"));
                q.AddTrigger(opts => opts
                    .ForJob(weeklyKey)
                    .WithIdentity("WeeklySummaryTrigger", "EmailJobs")
                    .WithCronSchedule("0 0 9 ? * MON")
                    .WithDescription("Trigger for weekly summary every Monday"));

                var reminderKey = new JobKey("TaskReminderJob", "EmailJobs");
                q.AddJob<TaskReminderJob>(opts => opts
                    .WithIdentity(reminderKey)
                    .WithDescription("Send task reminder every morning at 8:00 AM"));
                q.AddTrigger(opts => opts
                    .ForJob(reminderKey)
                    .WithIdentity("ReminderTrigger", "EmailJobs")
                    .WithCronSchedule("0 0 8 * * ?")
                    .WithDescription("Trigger for morning task reminder"));

                var clearKey = new JobKey("ClearExpiredDataJob", "MaintenanceJobs");
                q.AddJob<ClearExpiredDataJob>(opts => opts.WithIdentity(clearKey));
                q.AddTrigger(opts => opts
                    .ForJob(clearKey)
                    .WithCronSchedule("0 0 3 ? * MON")
                    .WithDescription("Clear expired blacklisted tokens and OTPs weekly"));
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
                options.AwaitApplicationStarted = true;
            });

            return services;
        }
    }
}
