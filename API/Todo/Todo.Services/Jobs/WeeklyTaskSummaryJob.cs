using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Services.Interfaces;

namespace Todo.Services.Jobs
{
    [DisallowConcurrentExecution]
    public class WeeklyTaskSummaryJob : IJob
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<WeeklyTaskSummaryJob> _logger;

        public WeeklyTaskSummaryJob(
            IEmailService emailService,
            ILogger<WeeklyTaskSummaryJob> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobKey = context.JobDetail.Key;
            _logger.LogInformation("Starting Weekly Task Summary Job: {JobKey} at {Time}",
                jobKey, DateTime.Now);

            try
            {
                await _emailService.SendWeeklyTaskSummaryAsync();

                _logger.LogInformation("Weekly Task Summary Job completed successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Weekly Task Summary Job failed!");
                throw new JobExecutionException(ex, refireImmediately: false);
            }
        }
    }
}
