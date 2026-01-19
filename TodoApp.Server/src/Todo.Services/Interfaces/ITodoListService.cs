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
    public interface ITodoListService
    {
        Task<AppResponse<TodoListResponse>> GetByIdAsync(Guid id);
        Task<AppResponse<TodoListResponse>> CreateAsync(TodoListRequest request);
        Task<AppResponse<TodoListResponse>> UpdateAsync(TodoListRequest request);
        Task<AppResponse<string>> DeleteAsync(Guid id);
        Task<AppResponse<SearchResponse<TodoListResponse>>> SearchAsync(SearchRequest request);
    }
}
