using MayNghien.Infrastructures.Models.Responses;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;

namespace Todo.Application.Interfaces.Reports
{
    public interface IGetProgressReportHandler
    {
        Task<AppResponse<TodoItemReportResponse>> HandleAsync(TodoItemReportRequest request);
    }
}
