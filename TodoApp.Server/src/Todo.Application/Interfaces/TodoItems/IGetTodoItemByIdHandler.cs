using MayNghien.Infrastructures.Models.Responses;
using Todo.DTOs.Responses;

namespace Todo.Application.Interfaces.TodoItems
{
    public interface IGetTodoItemByIdHandler
    {
        Task<AppResponse<TodoItemResponse>> HandleAsync(Guid id);
    }
}
