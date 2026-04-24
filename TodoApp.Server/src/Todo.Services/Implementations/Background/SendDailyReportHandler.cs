using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Requests;
using Todo.Services.Interfaces.Background;
using Todo.Services.Interfaces.Reports;

namespace Todo.Services.Implementations.Background
{
    public class SendDailyReportHandler : ISendDailyReportHandler
    {
        private readonly IGetProgressReportHandler _getReport;
        private readonly IEmailService _emailService;
        private readonly ILogger<SendDailyReportHandler> _logger;
        private readonly string _recipientEmail;

        public SendDailyReportHandler(IGetProgressReportHandler getReport, IEmailService emailService, ILogger<SendDailyReportHandler> logger, string recipientEmail)
        {
            _getReport = getReport;
            _emailService = emailService;
            _logger = logger;
            _recipientEmail = recipientEmail;
        }

        public async Task HandleAsync()
        {
            try
            {
                _logger.LogInformation("Sending daily report at {Time}", DateTime.Now);
                var report = await _getReport.HandleAsync(new TodoItemReportRequest());
                var body = $"""
                    Daily Todo Item Report - {DateTime.Now:dd-MM-yyyy}

                    Completed Today: {report.Data?.CompletedTasks}
                    In Progress: {report.Data?.InProgressTasks}
                    Total: {report.Data?.TotalTasks}
                 """;
                await _emailService.SendEmailAsync(
                    _recipientEmail,
                    $"Daily Task Report - {DateTime.Now:yyyy-MM-dd}",
                    body);
                _logger.LogInformation("Daily report sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send daily report");
                throw;
            }
        }
    }
}
