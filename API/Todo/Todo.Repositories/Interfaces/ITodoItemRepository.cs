using MayNghien.Infrastructures.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Todo.Models.Data;
using Todo.Models.Entities;

namespace Todo.Repositories.Interfaces
{
    public interface ITodoItemRepository : IGenericRepository<TodoItem, ApplicationDbContext, ApplicationUser>
    {
    }
}
