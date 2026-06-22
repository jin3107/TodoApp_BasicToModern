using MayNghien.Infrastructures.Repository;
using Todo.Models.Data;
using Todo.Domain.Entities;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;

namespace Todo.Repositories.Implementations
{
    public class TodoItemProgressReportRepository : GenericRepository<TodoItemProgressReport, ApplicationDbContext, ApplicationUser>, ITodoItemProgressReportReporitory
    {
        public TodoItemProgressReportRepository(ApplicationDbContext unitOfWork) : base(unitOfWork)
        {
        }
    }
}
