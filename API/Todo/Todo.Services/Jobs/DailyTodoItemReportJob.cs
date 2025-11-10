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
    public class DailyTodoItemReportJob : IJob
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<DailyTodoItemReportJob> _logger;

        public DailyTodoItemReportJob(IEmailService emailService, ILogger<DailyTodoItemReportJob> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobKey = context.JobDetail.Key;
            _logger.LogInformation("Starting Daily Todo Item Report Job: {JobKey} at {Time}", jobKey, DateTime.Now);
            try
            {
                await _emailService.SendDailyTodoItemReportAsync();
                _logger.LogInformation("Daily Todo Item Report Job completed successfilly");
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Daily Todo Item Report Job failed");
                throw new JobExecutionException(ex, refireImmediately: false);
            }
        }
    }
}
