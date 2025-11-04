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
    public class DailyTaskReportJob : IJob
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<DailyTaskReportJob> _logger;

        public DailyTaskReportJob(IEmailService emailService, ILogger<DailyTaskReportJob> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobKey = context.JobDetail.Key;
            _logger.LogInformation("Starting Daily Task Report Job: {JobKey} at {Time}", jobKey, DateTime.Now);
            try
            {
                await _emailService.SendDailyTaskReportAsync();
                _logger.LogInformation("Daily Task Report Job completed successfilly");
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Daily Task Report Job failed");
            }
        }
    }
}
