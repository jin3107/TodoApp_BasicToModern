using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Services.Interfaces;
using Todo.Services.Interfaces.Background;

namespace Todo.Services.Jobs
{
    [DisallowConcurrentExecution]
    public class TaskReminderJob : IJob
    {
        private readonly ISendReminderHandler _handler;
        private readonly ILogger<TaskReminderJob> _logger;

        public TaskReminderJob(ISendReminderHandler handler, ILogger<TaskReminderJob> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting Todo Item Reminder Job: {JobKey} at {Time}",
                context.JobDetail.Key, DateTime.Now);
            try
            {
                await _handler.HandleAsync();
                _logger.LogInformation("Todo Item Reminder Job completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Todo Item Reminder Job failed");
                throw new JobExecutionException(ex, refireImmediately: false);
            }
        }
    }
}
