using MayNghien.Infrastructures.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;

namespace Todo.Services.Interfaces.TodoLists
{
    public interface IUpdateTodoListHandler
    {
        Task<AppResponse<TodoListResponse>> HandleAsync(TodoListRequest request);
    }
}
