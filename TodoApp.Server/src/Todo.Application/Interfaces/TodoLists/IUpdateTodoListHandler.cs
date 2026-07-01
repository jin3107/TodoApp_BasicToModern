using MayNghien.Infrastructures.Models.Responses;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;

namespace Todo.Application.Interfaces.TodoLists
{
    public interface IUpdateTodoListHandler
    {
        Task<AppResponse<TodoListResponse>> HandleAsync(TodoListRequest request);
    }
}
