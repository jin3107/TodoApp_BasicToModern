using MayNghien.Infrastructures.Models.Responses;

namespace Todo.Application.Interfaces.TodoLists
{
    public interface IDeleteTodoListHandler
    {
        Task<AppResponse<string>> HandleAsync(Guid id);
    }
}
