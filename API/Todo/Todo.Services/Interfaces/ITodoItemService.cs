using MayNghien.Infrastructures.Models.Requests;
using MayNghien.Infrastructures.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;

namespace Todo.Services.Interfaces
{
    public interface ITodoItemService
    {
        Task<AppResponse<TodoItemResponse>> GetByIdAsync(Guid id);
        Task<AppResponse<TodoItemResponse>> CreateAsync(TodoItemRequest request);
        Task<AppResponse<TodoItemResponse>> UpdateAsync(TodoItemRequest request);
        Task<AppResponse<string>> DeleteAsync(Guid id);
        Task<AppResponse<SearchResponse<TodoItemResponse>>> SearchAsync(SearchRequest request);
    }
}
