using MayNghien.Infrastructures.Models.Requests;
using MayNghien.Infrastructures.Models.Responses;
using Todo.DTOs.Responses;

namespace Todo.Application.Interfaces.TodoLists
{
    public interface ISearchTodoListHandler
    {
        Task<AppResponse<SearchResponse<TodoListResponse>>> HandleAsync(SearchRequest request);
    }
}
