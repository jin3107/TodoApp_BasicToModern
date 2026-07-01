using MayNghien.Infrastructures.Models.Responses;

namespace Todo.Application.Interfaces.TodoItems
{
    public interface IDeleteTodoItemHandler
    {
        Task<AppResponse<string>> HandleAsync(Guid id);
    }
}
