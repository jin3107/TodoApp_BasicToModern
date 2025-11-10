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
            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ITodoItemService, TodoItemService>();
            services.AddScoped<ITodoItemReportService, TodoItemReportService>();
            services.AddScoped<IEmailService, EmailService>();
            return services;
        }
    }
}
