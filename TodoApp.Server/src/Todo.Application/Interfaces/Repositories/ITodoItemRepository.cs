using System.Linq.Expressions;
using MayNghien.Infrastructures.Models.Requests;
using Todo.Application.Common;
using Todo.Domain.Entities;

namespace Todo.Application.Interfaces.Repositories
{
    public interface ITodoItemRepository
    {
        Task<TodoItem?> GetByIdAsync(Guid id);
        Task AddAsync(TodoItem entity);
        Task UpdateAsync(TodoItem entity);
        Task<PagedResult<TodoItem>> SearchAsync(Expression<Func<TodoItem, bool>> predicate, SortByInfo? sortBy, int pageIndex, int pageSize);
        Task<List<TodoItem>> GetAllActiveAsync();
    }
}
