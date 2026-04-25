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
            services.AddScoped<ISendDailyReportHandler, SendDailyReportHandler>();
            services.AddScoped<ISendReminderHandler, SendReminderHandler>();
            services.AddScoped<ISendWeeklySummaryHandler, SendWeeklySummaryHandler>();

            return services;
        }
    }
}
