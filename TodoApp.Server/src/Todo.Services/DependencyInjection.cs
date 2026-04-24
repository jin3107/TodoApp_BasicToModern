using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Services.Implementations.Background;
using Todo.Services.Implementations.Reports;
using Todo.Services.Implementations.TodoItems;
using Todo.Services.Implementations.TodoLists;
using Todo.Services.Interfaces;
using Todo.Services.Interfaces.Background;
using Todo.Services.Interfaces.Reports;
using Todo.Services.Interfaces.TodoItems;
using Todo.Services.Interfaces.TodoLists;

namespace Todo.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Handlers todo items
            services.AddScoped<ICreateTodoItemHandler, CreateTodoItemHandler>();
            services.AddScoped<IUpdateTodoItemHandler, UpdateTodoItemHandler>();
            services.AddScoped<IDeleteTodoItemHandler, DeleteTodoItemHandler>();
            services.AddScoped<IGetTodoItemByIdHandler, GetTodoItemByIdHandler>();
            services.AddScoped<ISearchTodoItemHandler, SearchTodoItemHandler>();

            // Handlers todo lists
            services.AddScoped<ICreateTodoListHandler, CreateTodoListHandler>();
            services.AddScoped<IUpdateTodoListHandler, UpdateTodoListHandler>();
            services.AddScoped<IDeleteTodoListHandler, DeleteTodoListHandler>();
            services.AddScoped<IGetTodoListByIdHandler, GetTodoListByIdHandler>();
            services.AddScoped<ISearchTodoListHandler, SearchTodoListHandler>();


            // Report handlers 
            services.AddScoped<GetProgressReportHandler>();
            services.AddScoped<IGetProgressReportHandler>(sp =>
                new CachedGetProgressReportHandler(
                    sp.GetRequiredService<GetProgressReportHandler>(),
                    sp.GetRequiredService<ICacheService>(),
                    sp.GetRequiredService<ILogger<CachedGetProgressReportHandler>>()
                ));
            services.AddScoped<ICreateDailySnapshotHandler, CreateDailySnapshotHandler>();

            // Backgrounds
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISendDailyReportHandler>(sp =>
            {
                var recipientEmail = sp.GetRequiredService<IConfiguration>()["EmailSettings:RecipientEmail"]
                    ?? throw new InvalidOperationException("Missing configuration: EmailSettings:RecipientEmail");

                return new SendDailyReportHandler(
                    sp.GetRequiredService<IGetProgressReportHandler>(),
                    sp.GetRequiredService<IEmailService>(),
                    sp.GetRequiredService<ILogger<SendDailyReportHandler>>(),
                    recipientEmail);
            });
            services.AddScoped<ISendReminderHandler>(sp =>
            {
                var recipientEmail = sp.GetRequiredService<IConfiguration>()["EmailSettings:RecipientEmail"]
                    ?? throw new InvalidOperationException("Missing configuration: EmailSettings:RecipientEmail");

                return new SendReminderHandler(
                    sp.GetRequiredService<IGetProgressReportHandler>(),
                    sp.GetRequiredService<IEmailService>(),
                    sp.GetRequiredService<ILogger<SendReminderHandler>>(),
                    recipientEmail);
            });
            services.AddScoped<ISendWeeklySummaryHandler>(sp =>
            {
                var recipientEmail = sp.GetRequiredService<IConfiguration>()["EmailSettings:RecipientEmail"]
                    ?? throw new InvalidOperationException("Missing configuration: EmailSettings:RecipientEmail");

                return new SendWeeklySummaryHandler(
                    sp.GetRequiredService<IGetProgressReportHandler>(),
                    sp.GetRequiredService<IEmailService>(),
                    sp.GetRequiredService<ILogger<SendWeeklySummaryHandler>>(),
                    recipientEmail);
            });

            return services;
        }
    }
}
