using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Repositories.Implementations;
using Todo.Repositories.Interfaces;

namespace Todo.Repositories
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services) 
        {
            services.AddScoped<ITodoItemRepository, TodoItemRepository>();
            services.AddScoped<ITodoListRepository, TodoListRepository>();
            services.AddScoped<ITodoItemProgressReportReporitory, TodoItemProgressReportRepository>();

            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IBlacklistedTokenRepository, BlacklistedTokenRepository>();
            services.AddScoped<IOtpCodeRepository, OtpCodeRepository>();

            return services;
        }
    }
}
