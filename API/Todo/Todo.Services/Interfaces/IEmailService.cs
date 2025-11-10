using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendDailyTodoItemReportAsync();
        Task SendWeeklyTodoItemSummaryAsync();
        Task SendTodoItemReminderAsync();
        Task SendEmailAsync(string to, string subject, string body);
    }
}
