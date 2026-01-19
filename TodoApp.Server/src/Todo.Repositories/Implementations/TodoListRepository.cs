using MayNghien.Infrastructures.Repository;
using Microsoft.EntityFrameworkCore;
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
    public class TodoListRepository : GenericRepository<TodoList, ApplicationDbContext, ApplicationUser>, ITodoListRepository
    {
        public TodoListRepository(ApplicationDbContext unitOfWork) : base(unitOfWork)
        {
        }
    }
}