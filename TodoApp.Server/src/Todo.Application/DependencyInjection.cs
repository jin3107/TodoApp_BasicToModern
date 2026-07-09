using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Todo.Application.Implementations.Background;
using Todo.Application.Implementations.Reports;
using Todo.Application.Implementations.TodoItems;
using Todo.Application.Implementations.TodoLists;
using Todo.Application.Interfaces.Background;
using Todo.Application.Interfaces.Cache;
using Todo.Application.Interfaces.Reports;
using Todo.Application.Interfaces.TodoItems;
using Todo.Application.Interfaces.TodoLists;

namespace Todo.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // TodoItem handlers
            services.AddScoped<ICreateTodoItemHandler, CreateTodoItemHandler>();
            services.AddScoped<IUpdateTodoItemHandler, UpdateTodoItemHandler>();
            services.AddScoped<IDeleteTodoItemHandler, DeleteTodoItemHandler>();
            services.AddScoped<IGetTodoItemByIdHandler, GetTodoItemByIdHandler>();
            services.AddScoped<ISearchTodoItemHandler, SearchTodoItemHandler>();

            // TodoList handlers
            services.AddScoped<ICreateTodoListHandler, CreateTodoListHandler>();
            services.AddScoped<IUpdateTodoListHandler, UpdateTodoListHandler>();
            services.AddScoped<IDeleteTodoListHandler, DeleteTodoListHandler>();
            services.AddScoped<IGetTodoListByIdHandler, GetTodoListByIdHandler>();
            services.AddScoped<ISearchTodoListHandler, SearchTodoListHandler>();

            // Report handlers (decorator pattern: CachedGetProgressReportHandler wraps GetProgressReportHandler)
            services.AddScoped<GetProgressReportHandler>();
            services.AddScoped<IGetProgressReportHandler>(sp =>
                new CachedGetProgressReportHandler(
                    sp.GetRequiredService<GetProgressReportHandler>(),
                    sp.GetRequiredService<ICacheService>(),
                    sp.GetRequiredService<ILogger<CachedGetProgressReportHandler>>()
                ));
            services.AddScoped<ICreateDailySnapshotHandler, CreateDailySnapshotHandler>();

            // Background handlers
            services.AddScoped<IClearExpiredDataHandler, ClearExpiredDataHandler>();
            services.AddScoped<ISendDailyReportHandler, SendDailyReportHandler>();
            services.AddScoped<ISendReminderHandler, SendReminderHandler>();
            services.AddScoped<ISendWeeklySummaryHandler, SendWeeklySummaryHandler>();

            return services;
        }
    }
}
