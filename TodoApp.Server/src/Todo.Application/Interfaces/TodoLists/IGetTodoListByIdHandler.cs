using MayNghien.Infrastructures.Models.Responses;
using Todo.DTOs.Responses;

namespace Todo.Application.Interfaces.TodoLists
{
    public interface IGetTodoListByIdHandler
    {
        Task<AppResponse<TodoListResponse>> HandleAsync(Guid id);
    }
}
