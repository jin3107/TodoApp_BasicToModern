using MayNghien.Infrastructures.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Responses;

namespace Todo.Services.Interfaces.TodoItems
{
    public interface IGetTodoItemByIdHandler
    {
        Task<AppResponse<TodoItemResponse>> HandleAsync(Guid id);
    }
}
