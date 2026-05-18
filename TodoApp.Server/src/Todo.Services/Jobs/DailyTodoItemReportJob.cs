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
    public class DailyTodoItemReportJob : IJob
    {
        private readonly ISendDailyReportHandler _handler;
        private readonly ILogger<DailyTodoItemReportJob> _logger;

        public DailyTodoItemReportJob(ISendDailyReportHandler handler, ILogger<DailyTodoItemReportJob> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting Daily Todo Item Report Job: {JobKey} at {Time}",
                context.JobDetail.Key, DateTime.Now);
            try
            {
                await _handler.HandleAsync();
                _logger.LogInformation("Daily Todo Item Report Job completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Daily Todo Item Report Job failed");
                throw new JobExecutionException(ex, refireImmediately: false);
            }
        }
    }
}
