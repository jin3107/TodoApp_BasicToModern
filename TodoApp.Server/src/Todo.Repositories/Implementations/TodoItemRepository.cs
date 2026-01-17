using MayNghien.Infrastructures.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Models.Data;
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
