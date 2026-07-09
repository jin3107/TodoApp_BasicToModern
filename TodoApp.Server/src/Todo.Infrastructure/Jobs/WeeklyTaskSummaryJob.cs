using Quartz;
using Todo.Application.Interfaces.Background;

namespace Todo.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class WeeklyTaskSummaryJob : IJob
    {
        private readonly ISendWeeklySummaryHandler _handler;

        public WeeklyTaskSummaryJob(ISendWeeklySummaryHandler handler)
        {
            _handler = handler;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _handler.HandleAsync();
        }
    }
}
