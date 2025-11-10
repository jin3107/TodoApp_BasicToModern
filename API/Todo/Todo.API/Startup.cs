using Todo.Repositories.Implementations;
using Todo.Repositories.Interfaces;
using Todo.Services.Implementations;
using Todo.Services.Interfaces;

namespace Todo.API
{
    public class Startup
    {
        public void Mapping(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();
            builder.Services.AddScoped<ITodoItemProgressReportReporitory, TodoItemProgressReportRepository>();

            builder.Services.AddScoped<ITodoItemService, TodoItemService>();
            builder.Services.AddScoped<ITodoItemReportService, TodoItemReportService>();
        }
    }
}
