using MayNghien.Infrastructures.Models.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Services.Implementations;
using Todo.Services.Interfaces;
using Xunit;

namespace Todo.Tests
{
    public class EmailServiceTests
    {
        // ── Helpers ─────────────────────────────────────────────────────────────

        private static IConfiguration BuildFakeConfig() =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["EmailSettings:SmtpServer"]     = "localhost",
                    ["EmailSettings:SmtpPort"]       = "9999",
                    ["EmailSettings:SmtpUsername"]   = "test",
                    ["EmailSettings:SmtpPassword"]   = "test",
                    ["EmailSettings:FromEmail"]      = "from@test.dev",
                    ["EmailSettings:FromName"]       = "Test",
                    ["EmailSettings:RecipientEmail"] = "to@test.dev",
                })
                .Build();

        private static Mock<ITodoItemReportService> BuildReportServiceMock()
        {
            var mock = new Mock<ITodoItemReportService>();
            mock.Setup(s => s.GetProgressReportAsync(It.IsAny<TodoItemReportRequest>()))
                .ReturnsAsync(new AppResponse<TodoItemReportResponse>().BuildResult(
                    new TodoItemReportResponse
                    {
                        TotalTasks      = 10,
                        CompletedTasks  = 5,
                        InProgressTasks = 3,
                        OverdueTasks    = 2,
                        MostOverdueTasks = new List<TodoItemResponse>()
                    }));
            return mock;
        }

        // ── Test subclasses ──────────────────────────────────────────────────────

        /// Overrides SendEmailAsync to record that it was called without hitting SMTP.
        private class SpyEmailService : EmailService
        {
            public bool SendEmailWasCalled { get; private set; }

            public SpyEmailService(ITodoItemReportService reportService, IConfiguration config)
                : base(reportService, NullLogger<EmailService>.Instance, config) { }

            public override Task SendEmailAsync(string to, string subject, string body)
            {
                SendEmailWasCalled = true;
                return Task.CompletedTask;
            }
        }

        /// Overrides SendEmailAsync to throw, simulating an SMTP failure.
        private class ThrowingEmailService : EmailService
        {
            public ThrowingEmailService(ITodoItemReportService reportService, IConfiguration config)
                : base(reportService, NullLogger<EmailService>.Instance, config) { }

            public override Task SendEmailAsync(string to, string subject, string body) =>
                throw new InvalidOperationException("Simulated SMTP failure");
        }

        // ── Tests ────────────────────────────────────────────────────────────────

        /// Fails before fix: SendEmailAsync is never called — method silently returns.
        /// Passes after fix: SendEmailAsync is reached and the spy records the call.
        [Fact]
        public async Task WeeklySummary_ShouldCallSendEmail_WhenReportIsAvailable()
        {
            var sut = new SpyEmailService(BuildReportServiceMock().Object, BuildFakeConfig());

            await sut.SendWeeklyTodoItemSummaryAsync();

            Assert.True(sut.SendEmailWasCalled,
                "SendEmailAsync was never invoked. The weekly summary silently no-oped.");
        }

        /// Fails before fix: the catch block swallows the exception — method returns cleanly.
        /// Passes after fix: the exception propagates to the caller as expected.
        [Fact]
        public async Task WeeklySummary_ShouldPropagateException_WhenSmtpFails()
        {
            var sut = new ThrowingEmailService(BuildReportServiceMock().Object, BuildFakeConfig());

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.SendWeeklyTodoItemSummaryAsync());
        }
    }
}
