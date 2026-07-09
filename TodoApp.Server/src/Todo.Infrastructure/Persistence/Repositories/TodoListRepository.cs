using System.Linq.Expressions;
using MayNghien.Infrastructures.Models.Requests;
using Microsoft.EntityFrameworkCore;
using Todo.Application.Common;
using Todo.Application.Interfaces.Repositories;
using Todo.Domain.Entities;
using Todo.Models.Data;

namespace Todo.Infrastructure.Persistence.Repositories
{
    public class TodoListRepository : ITodoListRepository
    {
        private readonly ApplicationDbContext _context;

        public TodoListRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TodoList?> GetByIdAsync(Guid id)
            => await _context.TodoLists.FindAsync(id);

        public async Task AddAsync(TodoList entity)
        {
            entity.CreatedOn ??= DateTime.UtcNow;
            entity.CreatedBy ??= "";
            await _context.TodoLists.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TodoList entity)
        {
            _context.TodoLists.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<TodoList>> SearchAsync(Expression<Func<TodoList, bool>> predicate, SortByInfo? sortBy, int pageIndex, int pageSize)
        {
            var query = _context.TodoLists.Where(predicate);
            var totalCount = await query.CountAsync();

            var sorted = sortBy != null ? query.ApplySort(sortBy) : query.OrderBy(x => x.Name);
            var items = await sorted.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<TodoList> { Items = items, TotalCount = totalCount };
        }
    }
}
