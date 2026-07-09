using MayNghien.Infrastructures.Models.Responses;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;

namespace Todo.Application.Interfaces.TodoLists
{
    public interface ICreateTodoListHandler
    {
        Task<AppResponse<TodoListResponse>> HandleAsync(TodoListRequest request);
    }
}
