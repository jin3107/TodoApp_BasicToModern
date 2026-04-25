using MayNghien.Infrastructures.Models.Requests;
using MayNghien.Infrastructures.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Responses;

namespace Todo.Services.Interfaces.TodoItems
{
    public interface ISearchTodoItemHandler
    {
        Task<AppResponse<SearchResponse<TodoItemResponse>>> HandleAsync(SearchRequest request);
    }
}
