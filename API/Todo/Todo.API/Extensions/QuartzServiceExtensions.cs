using Quartz;
using Todo.Services.Jobs;

namespace Todo.API.Extensions
{
    public static class QuartzServiceExtensions
    {
        public static IServiceCollection AddQuartzConfiguration(this IServiceCollection services)
        {
            services.AddQuartz(q =>
            {
                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 3;
                });

                var dailyReportJobKey = new JobKey("DailyTaskReportJob", "EmailJobs");
                var dailyReportTrigger = new TriggerKey("DailyReportTrigger", "EmailJobs");
                q.AddJob<DailyTaskReportJob>(opts => opts
                    .WithIdentity(dailyReportJobKey)
                    .WithDescription("Send daily task report email at 6:00 PM"));

                q.AddTrigger(opts => opts
                    .ForJob(dailyReportJobKey)
                    .WithIdentity(dailyReportTrigger)
                    .WithCronSchedule("0 0 18 * * ?") // 6:00 PM mỗi ngày
                    .WithDescription("Trigger for daily report at 6:00 PM"));

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
