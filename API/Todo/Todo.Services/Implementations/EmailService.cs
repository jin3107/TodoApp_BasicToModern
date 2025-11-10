using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Services.Interfaces;

namespace Todo.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly ITodoItemReportService _taskReportService;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ITodoItemReportService taskReportService, ILogger<EmailService> logger, IConfiguration configuration)
        {
            _taskReportService = taskReportService;
            _logger = logger;
            _configuration = configuration;
        }

        private string SmtpServer => _configuration["EmailSettings:SmtpServer"]!;
        private int SmtpPort => int.Parse(_configuration["EmailSettings:SmtpPort"]!);
        private string SmtpUsername => _configuration["EmailSettings:SmtpUsername"]!;
        private string SmtpPassword => _configuration["EmailSettings:SmtpPassword"]!;
        private string FromEmail => _configuration["EmailSettings:FromEmail"]!;
        private string FromName => _configuration["EmailSettings:FromName"]!;
        private string RecipientEmail => _configuration["EmailSettings:RecipientEmail"]!;

        public async Task SendDailyTodoItemReportAsync()
        {
            try
            {
                _logger.LogInformation("Sending daily todo item report at {DateTime.Now}", DateTime.Now);

                var request = new TodoItemReportRequest();
                var reportResponse = await _taskReportService.GetProgressReportAsync(request);
                var emailBody = BuildDailyReportEmail(reportResponse.Data);
                await SendEmailAsync(
                    to: RecipientEmail,
                    subject: $"Daily Task Report - {DateTime.Now:yyyy-MM-dd}",
                    body: emailBody
                );

                _logger.LogInformation("Daily todo item report sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send daily todo item report");
                throw;
            }
        }

        private string BuildDailyReportEmail(TodoItemReportResponse report)
        {
            return $"""
                Daily todo item Report - {DateTime.Now:dd-MM-yyyy}
                
                Todo Items Completed Today: {report.CompletedTasks}
                Todo Items In Progress: {report.InProgressTasks}
                Todo Items Tasks: {report.TotalTasks}
                """;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(FromName, FromEmail));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;
                var bodyBuilder = new BodyBuilder
                {
                    TextBody = body,
                    HtmlBody = body.Replace("\n", "<br/>")
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(SmtpServer, SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(SmtpUsername, SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("Email sent to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }

        public async Task SendTodoItemReminderAsync()
        {
            try
            {
                _logger.LogInformation("Sending todo item reminder report at {Time}", DateTime.Now);

                var request = new TodoItemReportRequest();
                var reportResponse = await _taskReportService.GetProgressReportAsync(request);
                if (reportResponse?.Data == null)
                {
                    _logger.LogWarning("Cannot get todo item report data");
                    return;
                }

                var report = reportResponse.Data;
                var hasOverdueTasks = report.OverdueTasks > 0;
                var hasOverdueTaskList = report.MostOverdueTasks?.Any() ?? false;
                if (!hasOverdueTaskList && !hasOverdueTasks)
                {
                    _logger.LogWarning("No overdue item. Skip sending reminder email");
                    return;
                }
                var emailBody = BuildTodoItemReminderEmail(report);
                await SendEmailAsync(
                    to: RecipientEmail,
                    subject: $"Todo Item Reminder - {report.OverdueTasks} Overdue item",
                    body: emailBody
                ); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send todo item reminder");
                throw;
            }
        }

        private string BuildTodoItemReminderEmail(TodoItemReportResponse report)
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

        public async Task SendWeeklyTodoItemSummaryAsync()
        {
            try
            {
                _logger.LogInformation("Seding weekly todo item summary at {Time}", DateTime.Now);
                var request = new TodoItemReportRequest();
                var reportResponse = await _taskReportService.GetProgressReportAsync(request);
                var emailBody = BuildWeeklyReportEmail(reportResponse.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send weekly todo item summary");
            }
        }

        private string BuildWeeklyReportEmail(TodoItemReportResponse report)
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
