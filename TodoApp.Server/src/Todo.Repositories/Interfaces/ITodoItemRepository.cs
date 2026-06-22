using MayNghien.Infrastructures.Repository;
using Todo.Models.Data;
using Todo.Domain.Entities;
using Todo.Models.Entities;

namespace Todo.Repositories.Interfaces
{
    public interface ITodoItemRepository : IGenericRepository<TodoItem, ApplicationDbContext, ApplicationUser>
    {
    }
}
