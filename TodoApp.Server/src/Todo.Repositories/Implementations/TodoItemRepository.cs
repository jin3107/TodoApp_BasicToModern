using MayNghien.Infrastructures.Repository;
using Todo.Models.Data;
using Todo.Domain.Entities;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;

namespace Todo.Repositories.Implementations
{
    public class TodoItemRepository : GenericRepository<TodoItem, ApplicationDbContext, ApplicationUser>, ITodoItemRepository
    {
        public TodoItemRepository(ApplicationDbContext unitOfWork) : base(unitOfWork)
        {
        }
    }
}
