using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Services.Interfaces.Background;
using Todo.Services.Interfaces.Reports;

namespace Todo.Services.Implementations.Background
{
    public class SendWeeklySummaryHandler : ISendWeeklySummaryHandler
    {
        private readonly IGetProgressReportHandler _getReport;
        private readonly IEmailService _emailService;
        private readonly ILogger<SendWeeklySummaryHandler> _logger;
        private readonly string _recipientEmail;

        public SendWeeklySummaryHandler(IGetProgressReportHandler getReport, IEmailService emailService, ILogger<SendWeeklySummaryHandler> logger, string recipientEmail)
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
                _logger.LogInformation("Sending weekly summary at {Time}", DateTime.Now);
                var report = await _getReport.HandleAsync(new TodoItemReportRequest());
                var body = BuildWeeklyBody(report.Data);
                await _emailService.SendEmailAsync(
                    _recipientEmail,
                    $"Weekly Todo Summary - Week of {DateTime.Now:dd-MM-yyyy}",
                    body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send weekly summary");
            }
        }

        private string BuildWeeklyBody(TodoItemReportResponse report)
        {
            return $"""
                Weekly Todo Item Summary - Week of {DateTime.Now:dd-MM-yyyy}

                This Week's Achievements:
                Completed: {report.CompletedTasks} items
                In Progress: {report.InProgressTasks} items
                Total: {report.TotalTasks} items

                Productivity Score: {CalculateProductivityScore(report)}%
                """;
        }

        private int CalculateProductivityScore(TodoItemReportResponse report)
        {
            if (report == null) return 0;
            var total = report.TotalTasks;
            var completed = report.CompletedTasks;
            var result = 0;
            if (total > 0)
                result = (int)((completed / (double)total) * 100);
            else result = 0;
            return result;
        }
    }
}
