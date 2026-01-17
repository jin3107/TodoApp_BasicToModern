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
            _logger.LogInformation("Starting Weekly Todo Item Summary Job: {JobKey} at {Time}",
                jobKey, DateTime.Now);

            try
            {
                await _emailService.SendWeeklyTodoItemSummaryAsync();

                _logger.LogInformation("Weekly Todo Item Summary Job completed successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Weekly Todo Item Summary Job failed!");
                throw new JobExecutionException(ex, refireImmediately: false);
            }
        }
    }
}
