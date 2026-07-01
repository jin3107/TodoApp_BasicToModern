using Quartz;
using Todo.Application.Interfaces.Background;

namespace Todo.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class DailyTodoItemReportJob : IJob
    {
        private readonly ISendDailyReportHandler _handler;

        public DailyTodoItemReportJob(ISendDailyReportHandler handler)
        {
            _handler = handler;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _handler.HandleAsync();
        }
    }
}
