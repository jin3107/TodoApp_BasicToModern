using MayNghien.Infrastructures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.Models.Entities
{
    public class TodoList : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }

        public ICollection<TodoItem>? TodoItems { get; set; }
    }
}
