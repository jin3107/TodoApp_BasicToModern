using MayNghien.Infrastructures.Repository;
using Todo.Models.Data;
using Todo.Domain.Entities;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;

namespace Todo.Repositories.Implementations
{
    public class TodoListRepository : GenericRepository<TodoList, ApplicationDbContext, ApplicationUser>, ITodoListRepository
    {
        public TodoListRepository(ApplicationDbContext unitOfWork) : base(unitOfWork)
        {
        }
    }
}