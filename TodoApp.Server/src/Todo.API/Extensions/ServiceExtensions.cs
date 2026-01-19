using Todo.Repositories.Implementations;
using Todo.Repositories.Interfaces;
using Todo.Services.Implementations;
using Todo.Services.Interfaces;

namespace Todo.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<ITodoItemRepository, TodoItemRepository>();
            services.AddScoped<ITodoItemProgressReportReporitory, TodoItemProgressReportRepository>();
            services.AddScoped<ITodoListRepository, TodoListRepository>();
            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register base services first (for dependency injection into cached versions)
            services.AddScoped<TodoItemService>();
            services.AddScoped<TodoItemReportService>();
            
            // Register cached versions as interface implementations
            services.AddScoped<ITodoItemService, CacheTodoItemService>();
            services.AddScoped<ITodoItemReportService, CachedTodoItemReportService>();
            services.AddScoped<ITodoListService, TodoListService>();
            
            services.AddScoped<IEmailService, EmailService>();
            return services;
        }
    }
}
