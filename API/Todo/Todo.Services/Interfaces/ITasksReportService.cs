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
    public interface ITasksReportService
    {
        Task<AppResponse<TaskReportResponse>> GetProgressReportAsync(TaskReportRequest request); 
        Task<AppResponse<Guid>> CreateDailySnapshotAsync();
        Task<AppResponse<List<DailyCompletionTrend>>> GetCompletionTrendAsync(DateTime startDate, DateTime endDate);
    }
}
