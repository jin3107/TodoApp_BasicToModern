using Microsoft.Extensions.Configuration;
using Quartz;
using Todo.Services.Jobs;

namespace Todo.API.Extensions
{
    public static class QuartzServiceExtensions
    {
        public static IServiceCollection AddQuartzConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddQuartz(q =>
            {
                q.SchedulerName = "TodoScheduler";
                q.SchedulerId = "AUTO";

                q.UseSimpleTypeLoader();

                // Configure persistent store with MySQL for clustering
                q.UsePersistentStore(store =>
                {
                    store.UseProperties = true;
                    store.UseMySql(configuration.GetConnectionString("DefaultConnection"));
                    store.UseSystemTextJsonSerializer();
                    store.UseClustering(cluster =>
                    {
                        cluster.CheckinInterval = TimeSpan.FromSeconds(20);
                        cluster.CheckinMisfireThreshold = TimeSpan.FromSeconds(40);
                    });
                });

                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 3;
                });

                var dailyReportJobKey = new JobKey("DailyTaskReportJob", "EmailJobs");
                var dailyReportTrigger = new TriggerKey("DailyReportTrigger", "EmailJobs");
                q.AddJob<DailyTodoItemReportJob>(opts => opts
                    .WithIdentity(dailyReportJobKey)
                    .WithDescription("Send daily task report email at 6:00 PM"));

                q.AddTrigger(opts => opts
                    .ForJob(dailyReportJobKey)
                    .WithIdentity(dailyReportTrigger)
                    .WithCronSchedule("0 0 18 * * ?") // 6:00 PM mỗi ngày
                    .WithDescription("Trigger for daily report at 6:00 PM"));

                var weeklySummaryJobKey = new JobKey("WeeklyTaskSummaryJob", "EmailJobs");
                var weeklySummaryReportTrigger = new TriggerKey("WeeklySummaryTrigger", "EmailJobs");
                q.AddJob<WeeklyTaskSummaryJob>(opts => opts
                    .WithIdentity(weeklySummaryJobKey)
                    .WithDescription("Send weekly task summary every Monday at 9:00 AM"));

                q.AddTrigger(opts => opts
                    .ForJob(weeklySummaryJobKey)
                    .WithIdentity(weeklySummaryReportTrigger)
                    .WithCronSchedule("0 0 9 ? * MON") // 9:00 am thứ 2
                    .WithDescription("Trigger for weekly summary every Monday"));

                var reminderJobKey = new JobKey("TaskReminderJob", "EmailJobs");
                var reminderReportTrigger = new TriggerKey("ReminderTrigger", "EmailJobs");
                q.AddJob<TaskReminderJob>(opts => opts
                    .WithIdentity(reminderJobKey)
                    .WithDescription("Send task reminder every morning at 8:00 AM"));

                q.AddTrigger(opts => opts
                    .ForJob(reminderJobKey)
                    .WithIdentity(reminderReportTrigger)
                    .WithCronSchedule("0 0 8 * * ?") // 8:00 am mỗi ngày
                    .WithDescription("Trigger for morning task reminder"));

                var clearJobKey = new JobKey("ClearExpiredDataJob", "MaintenanceJobs");
                var clearTrigger = new TriggerKey("ClearExpiredDataTrigger", "MaintenanceJobs");
                q.AddJob<ClearExpiredDataJob>(opts => opts
                    .WithIdentity(clearJobKey)
                    .WithDescription("Clear expired blacklisted tokens and OTPs weekly"));
                q.AddTrigger(opts => opts
                    .ForJob(clearJobKey)
                    .WithIdentity(clearTrigger)
                    .WithCronSchedule("0 0 3 ? * MON") // 3:00 AM thứ 2
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
