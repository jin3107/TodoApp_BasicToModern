using MayNghien.Infrastructures.Models.Responses;
using Microsoft.Extensions.Logging;
using Todo.Domain.Enums;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Application.Interfaces.Repositories;
using Todo.Application.Interfaces.Reports;
using Todo.Application.Mapping;

namespace Todo.Application.Implementations.Reports
{
    public class GetProgressReportHandler : IGetProgressReportHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly ILogger<GetProgressReportHandler> _logger;

        public GetProgressReportHandler(ITodoItemRepository todoItemRepository, ILogger<GetProgressReportHandler> logger)
        {
            _todoItemRepository = todoItemRepository;
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

                var activeTasks = await _todoItemRepository.GetAllActiveAsync();

                var filtered = activeTasks.Where(t =>
                    (t.CreatedOn.HasValue && t.CreatedOn.Value >= startDate && t.CreatedOn.Value < endExclusive) ||
                    (t.IsCompleted && t.CompletedOn.HasValue && t.CompletedOn.Value >= startDate && t.CompletedOn.Value < endExclusive)
                ).ToList();

                var totalTasks = filtered.Count;
                var completedTasks = filtered.Count(t => t.IsCompleted);
                var inProgressTasks = filtered.Count(t => !t.IsCompleted && t.DueDate >= today);
                var overdueTasks = filtered.Count(t => !t.IsCompleted && t.DueDate < today);

                var highPriorityPending = filtered.Count(t => !t.IsCompleted && t.Priority == Tier.High);
                var mediumPriorityPending = filtered.Count(t => !t.IsCompleted && t.Priority == Tier.Medium);
                var lowPriorityPending = filtered.Count(t => !t.IsCompleted && t.Priority == Tier.Low);

                var completionRate = totalTasks > 0
                    ? Math.Round((decimal)completedTasks / totalTasks * 100, 2)
                    : 0;

                var tasksWithCompletionTime = filtered
                    .Where(t => t.IsCompleted && t.CompletedOn.HasValue && t.CreatedOn.HasValue)
                    .ToList();
                var avgCompletionTime = tasksWithCompletionTime.Any()
                    ? (decimal)tasksWithCompletionTime.Average(t => (t.CompletedOn!.Value - t.CreatedOn!.Value).TotalHours)
                    : 0;

                var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                var startOfMonth = new DateTime(now.Year, now.Month, 1);

                var tasksCompletedToday = activeTasks.Count(t =>
                    t.IsCompleted && t.CompletedOn.HasValue && t.CompletedOn.Value >= today && t.CompletedOn.Value < today.AddDays(1));
                var tasksCompletedThisWeek = activeTasks.Count(t =>
                    t.IsCompleted && t.CompletedOn.HasValue && t.CompletedOn.Value >= startOfWeek);
                var tasksCompletedThisMonth = activeTasks.Count(t =>
                    t.IsCompleted && t.CompletedOn.HasValue && t.CompletedOn.Value >= startOfMonth);

                var mostOverdueTasks = filtered
                    .Where(t => !t.IsCompleted && t.DueDate < today)
                    .OrderBy(t => t.DueDate)
                    .Take(5)
                    .Select(TodoItemMapper.ToResponse)
                    .ToList();

                var priorityDistribution = new PriorityDistribution
                {
                    HighPriority = filtered.Count(t => t.Priority == Tier.High),
                    MediumPriority = filtered.Count(t => t.Priority == Tier.Medium),
                    LowPriority = filtered.Count(t => t.Priority == Tier.Low)
                };

                var createdByDate = filtered
                    .Where(t => t.CreatedOn.HasValue)
                    .GroupBy(t => t.CreatedOn!.Value.Date)
                    .ToDictionary(g => g.Key, g => g.Count());
                var completedByDate = filtered
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
