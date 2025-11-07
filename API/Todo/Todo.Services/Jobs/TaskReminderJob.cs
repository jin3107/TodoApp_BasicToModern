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
    public class TaskReminderJob : IJob
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<TaskReminderJob> _logger;

        public TaskReminderJob(IEmailService emailService, ILogger<TaskReminderJob> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobKey = context.JobDetail.Key;
            _logger.LogInformation("Starting Task Reminder Job: {JoKey} at {Time}", jobKey, DateTime.Now);
            try
            {
                await _emailService.SendTaskReminderAsync();
                _logger.LogInformation("Task Reminder Job completed successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Task Reminder Job failed!");
                throw new JobExecutionException(ex, refireImmediately: false);
            }
        }
    }
}
