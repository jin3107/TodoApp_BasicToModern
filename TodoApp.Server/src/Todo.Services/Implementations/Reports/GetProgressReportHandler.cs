using MayNghien.Infrastructures.Models.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Domain.Enums;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces;
using Todo.Services.Interfaces.Reports;
using Todo.Services.Mapping;

namespace Todo.Services.Implementations.Reports
{
    public class GetProgressReportHandler : IGetProgressReportHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly ITodoItemProgressReportReporitory _reportRepository;
        private readonly ILogger<GetProgressReportHandler> _logger;

        public GetProgressReportHandler(ITodoItemRepository todoItemRepository, ITodoItemProgressReportReporitory reportRepository, ILogger<GetProgressReportHandler> logger)
        {
            _todoItemRepository = todoItemRepository;
            _reportRepository = reportRepository;
            _logger = logger;
        }

        public async Task<AppResponse<TodoItemReportResponse>> HandleAsync(TodoItemReportRequest request)
        {
            var result = new AppResponse<TodoItemReportResponse>();
            try
            {
                var now = DateTime.UtcNow;
                var today = now.Date;
                var startDate = (request.StartDate ?? now.AddDays(-29)).Date;
                var endDate = (request.EndDate ?? now).Date;
                var endExclusive = endDate.AddDays(1);

                var baseQuery = _todoItemRepository.AsQueryable()
                    .Where(t => t.IsDeleted == false)
                    .AsNoTracking();

                var filteredQuery = baseQuery.Where(t =>
                    (t.CreatedOn.HasValue && t.CreatedOn.Value >= startDate && t.CreatedOn.Value < endExclusive) ||
                    (t.IsCompleted && t.CompletedOn.HasValue && t.CompletedOn.Value >= startDate && t.CompletedOn.Value < endExclusive)
                );

                var totalTasks = await filteredQuery.CountAsync();
                var completedTasks = await filteredQuery.CountAsync(t => t.IsCompleted);
                var inProgressTasks = await filteredQuery.CountAsync(t => !t.IsCompleted && t.DueDate >= today);
                var overdueTasks = await filteredQuery.CountAsync(t => !t.IsCompleted && t.DueDate < today);

                var highPriorityPending = await filteredQuery.CountAsync(t => !t.IsCompleted && t.Priority == Tier.High);
                var mediumPriorityPending = await filteredQuery.CountAsync(t => !t.IsCompleted && t.Priority == Tier.Medium);
                var lowPriorityPending = await filteredQuery.CountAsync(t => !t.IsCompleted && t.Priority == Tier.Low);

                var completionRate = totalTasks > 0
                    ? Math.Round((decimal)completedTasks / totalTasks * 100, 2)
                    : 0;

                var tasksWithCompletionTime = filteredQuery
                    .Where(t => t.IsCompleted && t.CompletedOn.HasValue && t.CreatedOn.HasValue)
                    .Select(t => new { t.CompletedOn, t.CreatedOn })
                    .ToList();
                var avgCompletionTime = tasksWithCompletionTime.Any()
                    ? (decimal)tasksWithCompletionTime.Average(t => (t.CompletedOn!.Value - t.CreatedOn!.Value).TotalHours)
                    : 0;

                var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                var startOfMonth = new DateTime(now.Year, now.Month, 1);

                var tasksCompletedToday = await baseQuery.CountAsync(t =>
                    t.IsCompleted && t.CompletedOn.HasValue && t.CompletedOn.Value >= today && t.CompletedOn.Value < today.AddDays(1));
                var tasksCompletedThisWeek = await baseQuery.CountAsync(t =>
                    t.IsCompleted && t.CompletedOn.HasValue && t.CompletedOn.Value >= startOfWeek);
                var tasksCompletedThisMonth = await baseQuery.CountAsync(t =>
                    t.IsCompleted && t.CompletedOn.HasValue && t.CompletedOn.Value >= startOfMonth);

                var mostOverdueEntities = await filteredQuery
                    .Where(t => !t.IsCompleted && t.DueDate < today)
                    .OrderBy(t => t.DueDate)
                    .Take(5)
                    .ToListAsync();
                var mostOverdueTasks = mostOverdueEntities
                    .Select(TodoItemMapper.ToResponse)
                    .ToList();

                var priorityDistribution = new PriorityDistribution
                {
                    HighPriority = await filteredQuery.CountAsync(t => t.Priority == Tier.High),
                    MediumPriority = await filteredQuery.CountAsync(t => t.Priority == Tier.Medium),
                    LowPriority = await filteredQuery.CountAsync(t => t.Priority == Tier.Low)
                };

                var trendItems = await baseQuery
                    .Where(t =>
                        (t.CreatedOn.HasValue && t.CreatedOn.Value >= startDate && t.CreatedOn.Value < endExclusive) ||
                        (t.IsCompleted && t.CompletedOn.HasValue && t.CompletedOn.Value >= startDate && t.CompletedOn.Value < endExclusive)
                    )
                    .Select(t => new { t.CreatedOn, t.CompletedOn, t.IsCompleted })
                    .ToListAsync();

                var createdByDate = trendItems
                    .Where(t => t.CreatedOn.HasValue)
                    .GroupBy(t => t.CreatedOn!.Value.Date)
                    .ToDictionary(g => g.Key, g => g.Count());
                var completedByDate = trendItems
                    .Where(t => t.IsCompleted && t.CompletedOn.HasValue)
                    .GroupBy(t => t.CompletedOn!.Value.Date)
                    .ToDictionary(g => g.Key, g => g.Count());

                var dayCount = (endDate - startDate).Days + 1;
                var completionTrend = Enumerable.Range(0, dayCount)
                    .Select(i => startDate.AddDays(i))
                    .Select(date => new DailyCompletionTrend
                    {
                        Date = date,
                        CompletedCount = completedByDate.GetValueOrDefault(date, 0),
                        CreatedCount = createdByDate.GetValueOrDefault(date, 0)
                    })
                    .ToList();

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
