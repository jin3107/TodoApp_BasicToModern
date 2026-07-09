using Quartz;
using Todo.Application.Interfaces.Background;

namespace Todo.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class TaskReminderJob : IJob
    {
        private readonly ISendReminderHandler _handler;

        public TaskReminderJob(ISendReminderHandler handler)
        {
            _handler = handler;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _handler.HandleAsync();
        }
    }
}
