using Todo.Domain.Entities;

namespace Todo.Application.Interfaces.Repositories
{
    public interface ITodoItemProgressReportRepository
    {
        Task AddAsync(TodoItemProgressReport entity);
    }
}
