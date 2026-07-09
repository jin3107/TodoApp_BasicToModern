using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Todo.Application.Interfaces.Background;

namespace Todo.Infrastructure.Implementations.Background
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        private string SmtpServer => _configuration["EmailSettings:SmtpServer"]!;
        private int SmtpPort => int.Parse(_configuration["EmailSettings:SmtpPort"]!);
        private string SmtpUsername => _configuration["EmailSettings:SmtpUsername"]!;
        private string SmtpPassword => _configuration["EmailSettings:SmtpPassword"]!;
        private string FromEmail => _configuration["EmailSettings:FromEmail"]!;
        private string FromName => _configuration["EmailSettings:FromName"]!;

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
    }
}
