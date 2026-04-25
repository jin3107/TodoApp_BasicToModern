using Microsoft.Extensions.Configuration;
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
    public class SendReminderHandler : ISendReminderHandler
    {
        private readonly IGetProgressReportHandler _getReport;
        private readonly IEmailService _emailService;
        private readonly ILogger<SendReminderHandler> _logger;
        private readonly string _recipientEmail;

        public SendReminderHandler(IGetProgressReportHandler getReport, 
            IEmailService emailService, ILogger<SendReminderHandler> logger,
            IConfiguration configuration)
        {
            _getReport = getReport;
            _emailService = emailService;
            _logger = logger;
            _recipientEmail = configuration["EmailSettings:RecipientEmail"]
                ?? throw new InvalidOperationException("Missing config: EmailSettings:RecipientEmail");
        }

        public async Task HandleAsync()
        {
            try
            {
                var reportResponse = await _getReport.HandleAsync(new TodoItemReportRequest());
                if (reportResponse?.Data == null)
                {
                    _logger.LogWarning("Cannot get report data");
                    return;
                }

                var report = reportResponse.Data;
                if (report.OverdueTasks == 0 && !(report.MostOverdueTasks?.Any() ?? false))
                {
                    _logger.LogWarning("No overdue items. Skip reminder");
                    return;
                }

                var body = BuildReminderBody(report);
                await _emailService.SendEmailAsync(
                    _recipientEmail,
                    $"Todo Item Reminder - {report.OverdueTasks} Overdue item",
                    body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reminder");
                throw;
            }
        }

        private string BuildReminderBody(TodoItemReportResponse report)
        {
            var overdueCount = report.OverdueTasks;
            var overdueTasks = report.MostOverdueTasks;
            var taskList = "";
            if (overdueTasks.Any())
            {
                taskList = "\nMost Overdue Todo Items:\n";
                var count = 1;
                foreach (var task in overdueTasks.Take(5))
                {
                    var dueDate = task.DueDate.ToString("dd-MM-yyyy");
                    var priority = task.Priority.ToString();
                    string priorityIcon = "";
                    switch (priority)
                    {
                        case "High":
                            priorityIcon = "🔴"; break;

                        case "Medium":
                            priorityIcon = "🟡"; break;

                        case "Low":
                            priorityIcon = "🟢"; break;

                        default:
                            priorityIcon = "⚪"; break;
                    }
                    taskList += $"{count}. {priorityIcon} {task.Title}\n";
                    taskList += $"Due: {dueDate} | Priority: {priority}\n\n";
                    count++;
                }
            }

            return $"""
                Todo Item Reminder - {DateTime.Now:dd-MM-yyyy}
                
                You have {overdueCount} overdue item(s) that need attention!
                {taskList}
                Current Status:
                Completed: {report.CompletedTasks} items
                In Progress: {report.InProgressTasks} items
                Overdue: {overdueCount} items
                """;
        }
    }
}
