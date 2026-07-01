using MayNghien.Infrastructures.Models.Requests;
using MayNghien.Infrastructures.Models.Responses;
using Todo.DTOs.Responses;

namespace Todo.Application.Interfaces.TodoItems
{
    public interface ISearchTodoItemHandler
    {
        Task<AppResponse<SearchResponse<TodoItemResponse>>> HandleAsync(SearchRequest request);
    }
}
