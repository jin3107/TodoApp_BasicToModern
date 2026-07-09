using Todo.Application.Interfaces.Repositories;
using Todo.Domain.Entities;
using Todo.Models.Data;

namespace Todo.Infrastructure.Persistence.Repositories
{
    public class TodoItemProgressReportRepository : ITodoItemProgressReportRepository
    {
        private readonly ApplicationDbContext _context;

        public TodoItemProgressReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TodoItemProgressReport entity)
        {
            entity.CreatedOn ??= DateTime.UtcNow;
            entity.CreatedBy ??= "";
            await _context.TaskProgressReports.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
    }
}
