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
            builder.Services.AddScoped<ITaskRepository, TaskRepository>();
            builder.Services.AddScoped<ITaskProgressReportReporitory, TaskProgressReportRepository>();

            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<ITasksReportService, TaskReportService>();
        }
    }
}
