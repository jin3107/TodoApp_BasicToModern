using System.Linq.Expressions;
using MayNghien.Infrastructures.Models.Requests;
using Todo.Application.Common;
using Todo.Domain.Entities;

namespace Todo.Application.Interfaces.Repositories
{
    public interface ITodoListRepository
    {
        Task<TodoList?> GetByIdAsync(Guid id);
        Task AddAsync(TodoList entity);
        Task UpdateAsync(TodoList entity);
        Task<PagedResult<TodoList>> SearchAsync(Expression<Func<TodoList, bool>> predicate, SortByInfo? sortBy, int pageIndex, int pageSize);
    }
}
