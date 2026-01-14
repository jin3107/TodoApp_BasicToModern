using MayNghien.Infrastructures.Models.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Commons.Enums;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces;
using Todo.Services.Mapping;

namespace Todo.Services.Implementations
{
    public class TodoItemReportService : ITodoItemReportService
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly ITodoItemProgressReportReporitory _reportRepository;

        public TodoItemReportService(ITodoItemRepository todoItemRepository, ITodoItemProgressReportReporitory reportRepository)
        {
            _todoItemRepository = todoItemRepository;
            _reportRepository = reportRepository;
        }

        public async Task<AppResponse<Guid>> CreateDailySnapshotAsync()
        {
            var result = new AppResponse<Guid>();
            try
            {
                var reportRequest = new TodoItemReportRequest();
                var reportResponse = await GetProgressReportAsync(reportRequest);
                if (!reportResponse.IsSuccess || reportResponse.Data == null)
                    return result.BuildError("Failed to generate report for snapshot");

                var report = reportResponse.Data;
                var now = DateTime.UtcNow;
                var snapshot = TodoItemReportMapper.ToEntity(report, now.Date, "Auto-generated daily snapshot");

                await _reportRepository.AddAsync(snapshot);
                result.BuildResult(snapshot.Id, "Daily snapshot created successfully.");
            }
            catch (Exception ex)
            {
                result.BuildError($"Error creating daily snapshot: {ex.Message}");
            }
            return result;
        }

        public async Task<AppResponse<TodoItemReportResponse>> GetProgressReportAsync(TodoItemReportRequest request)
        {
            var result = new AppResponse<TodoItemReportResponse>();
            try
            {
                var now = DateTime.UtcNow;
                
                // Lấy TẤT CẢ tasks (không filter theo thời gian ở đây)
                var allTasksQuery = _todoItemRepository.AsQueryable()
                    .Where(t => t.IsDeleted == false)
                    .AsNoTracking();

                var allTasks = await allTasksQuery.ToListAsync();
                
                // Xác định khoảng thời gian để filter
                var startDate = request.StartDate ?? now.AddDays(-29);
                var endDate = request.EndDate ?? now;

                // Filter tasks trong khoảng thời gian (được tạo hoặc hoàn thành)
                var filteredTasks = allTasks.Where(t =>
                    (t.CreatedOn.HasValue && t.CreatedOn.Value.Date >= startDate.Date && t.CreatedOn.Value.Date <= endDate.Date) ||
                    (t.IsCompleted && t.CompletedOn.HasValue && t.CompletedOn.Value.Date >= startDate.Date && t.CompletedOn.Value.Date <= endDate.Date)
                ).ToList();

                var totalTasks = filteredTasks.Count;
                var completedTasks = filteredTasks.Count(t => t.IsCompleted);
                var inProgressTasks = filteredTasks.Count(t => !t.IsCompleted && t.DueDate.Date >= now.Date);
                var overdueTasks = filteredTasks.Count(t => !t.IsCompleted && t.DueDate.Date < now.Date);

                var highPriorityPending = filteredTasks.Count(t => !t.IsCompleted && t.Priority == Tier.High);
                var mediumPriorityPending = filteredTasks.Count(t => !t.IsCompleted && t.Priority == Tier.Medium);
                var lowPriorityPending = filteredTasks.Count(t => !t.IsCompleted && t.Priority == Tier.Low);

                var completionRate = totalTasks > 0 ? Math.Round((decimal)completedTasks / totalTasks * 100, 2) : 0;

                var tasksWithCompletionTime = filteredTasks.Where(t => t.IsCompleted && t.CompletedOn.HasValue && t.CreatedOn.HasValue).ToList();
                var avgCompletionTime = tasksWithCompletionTime.Any() ? (decimal)tasksWithCompletionTime
                    .Average(t => (t.CompletedOn!.Value - t.CreatedOn!.Value).TotalHours) : 0;

                var today = now.Date;
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                
                // Stats "Hôm nay", "Tuần này", "Tháng này" vẫn dùng allTasks (không filter)
                var tasksCompletedToday = allTasks.Count(t => t.IsCompleted && t.CompletedOn.HasValue
                    && t.CompletedOn.Value.Date == today);
                var tasksCompletedThisWeek = allTasks.Count(t => t.IsCompleted && t.CompletedOn.HasValue
                    && t.CompletedOn.Value.Date >= startOfWeek);
                var tasksCompletedThisMonth = allTasks.Count(t => t.IsCompleted && t.CompletedOn.HasValue
                    && t.CompletedOn.Value.Date >= startOfMonth);

                var mostOverdueTasks = filteredTasks.Where(t => !t.IsCompleted && t.DueDate.Date < today)
                    .OrderBy(t => t.DueDate)
                    .Take(5).Select(TodoItemMapper.ToResponse).ToList();

                var priorityDistribution = new PriorityDistribution
                {
                    HighPriority = filteredTasks.Count(t => t.Priority == Tier.High),
                    MediumPriority = filteredTasks.Count(t => t.Priority == Tier.Medium),
                    LowPriority = filteredTasks.Count(t => t.Priority == Tier.Low)
                };

                var dayCount = (endDate.Date - startDate.Date).Days + 1;
                var dateRange = Enumerable.Range(0, dayCount).Select(i => startDate.Date.AddDays(i)).ToList();
                
                // CompletionTrend dùng allTasks nhưng filter theo date
                var completionTrend = dateRange.Select(date => new DailyCompletionTrend
                {
                    Date = date,
                    CompletedCount = allTasks.Count(t =>
                        t.IsCompleted && t.CompletedOn.HasValue && t.CompletedOn.Value.Date == date),
                    CreatedCount = allTasks.Count(t =>
                        t.CreatedOn.HasValue && t.CreatedOn.Value.Date == date)
                }).ToList();

                var report = new TodoItemReportResponse
                {
                    TotalTasks = totalTasks,
                    CompletedTasks = completedTasks,
                    InProgressTasks = inProgressTasks,
                    OverdueTasks = overdueTasks,
                    HighPriorityPendingTasks = highPriorityPending,
                    MediumPriorityPendingTasks = mediumPriorityPending,
                    LowPriorityPendingTasks = lowPriorityPending,
                    CompletionRate = completionRate,
                    AverageCompletionTimeHours = Math.Round(avgCompletionTime, 2),
                    TasksCompletedThisToday = tasksCompletedToday,
                    TasksCompletedThisWeek = tasksCompletedThisWeek,
                    TasksCompletedThisMonth = tasksCompletedThisMonth,
                    MostOverdueTasks = mostOverdueTasks,
                    PriorityDistribution = priorityDistribution,
                    CompletionTrend = completionTrend
                };

                result.BuildResult(report, "Report generated successfully.");
            }
            catch (Exception ex)
            {
                result.BuildError($"Error generating report: {ex.Message}");
            }
            return result;
        }
    }
}
