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
        private readonly ITasksReportService _taskReportService;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ITasksReportService taskReportService, ILogger<EmailService> logger, IConfiguration configuration)
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

        public async Task SendDailyTaskReportAsync()
        {
            try
            {
                _logger.LogInformation("Sending daily task report at {Time}", DateTime.Now);

                var request = new TaskReportRequest();
                var reportResponse = await _taskReportService.GetProgressReportAsync(request);
                var emailBody = BuildDailyReportEmail(reportResponse.Data);
                await SendEmailAsync(
                    to: RecipientEmail,
                    subject: $"Daily Task Report - {DateTime.Now:yyyy-MM-dd}",
                    body: emailBody
                );

                _logger.LogInformation("Daily task report sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send daily task report");
                throw;
            }
        }

        private string BuildDailyReportEmail(TaskReportResponse report)
        {
            return $"""
                Daily Task Report - {DateTime.Now:dd MM yyyy}
                
                Tasks Completed Today: {report.CompletedTasks}
                Tasks In Progress: {report.InProgressTasks}
                Total Tasks: {report.TotalTasks}
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

        public Task SendTaskReminderAsync()
        {
            throw new NotImplementedException();
        }

        public Task SendWeeklyTaskSummaryAsync()
        {
            throw new NotImplementedException();
        }
    }
}
