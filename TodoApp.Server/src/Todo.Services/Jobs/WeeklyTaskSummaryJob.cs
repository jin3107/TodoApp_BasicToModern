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
    public class WeeklyTaskSummaryJob : IJob
    {
        private readonly ISendWeeklySummaryHandler _handler;
        private readonly ILogger<WeeklyTaskSummaryJob> _logger;

        public WeeklyTaskSummaryJob(ISendWeeklySummaryHandler handler, ILogger<WeeklyTaskSummaryJob> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting Weekly Todo Item Summary Job: {JobKey} at {Time}",
                context.JobDetail.Key, DateTime.Now);
            try
            {
                await _handler.HandleAsync();
                _logger.LogInformation("Weekly Todo Item Summary Job completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Weekly Todo Item Summary Job failed");
                throw new JobExecutionException(ex, refireImmediately: false);
            }
        }
    }
}
