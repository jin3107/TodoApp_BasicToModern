using System.Linq.Expressions;
using MayNghien.Infrastructures.Models.Requests;
using Microsoft.EntityFrameworkCore;
using Todo.Application.Common;
using Todo.Application.Interfaces.Repositories;
using Todo.Domain.Entities;
using Todo.Models.Data;

namespace Todo.Infrastructure.Persistence.Repositories
{
    public class TodoItemRepository : ITodoItemRepository
    {
        private readonly ApplicationDbContext _context;

        public TodoItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TodoItem?> GetByIdAsync(Guid id)
            => await _context.TodoItems.FindAsync(id);

        public async Task AddAsync(TodoItem entity)
        {
            entity.CreatedOn ??= DateTime.UtcNow;
            entity.CreatedBy ??= "";
            await _context.TodoItems.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TodoItem entity)
        {
            _context.TodoItems.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<TodoItem>> SearchAsync(Expression<Func<TodoItem, bool>> predicate, SortByInfo? sortBy, int pageIndex, int pageSize)
        {
            var query = _context.TodoItems.Where(predicate);
            var totalCount = await query.CountAsync();

            var sorted = sortBy != null ? query.ApplySort(sortBy) : query.OrderBy(x => x.Title);
            var items = await sorted.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<TodoItem> { Items = items, TotalCount = totalCount };
        }

        public async Task<List<TodoItem>> GetAllActiveAsync()
            => await _context.TodoItems.AsNoTracking().Where(t => !t.IsDeleted).ToListAsync();
    }
}
