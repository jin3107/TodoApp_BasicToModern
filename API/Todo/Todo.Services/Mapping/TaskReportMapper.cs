using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Responses;
using Todo.Models.Entities;

namespace Todo.Services.Mapping
{
    public static class TaskReportMapper
    {
        public static TaskProgressReport ToEntity(TaskReportResponse response, DateTime reportDate, string notes = "")
        {
            return new TaskProgressReport
            {
                Id = Guid.NewGuid(),
                ReportDate = reportDate,
                TotalTasks = response.TotalTasks,
                CompletedTasks = response.CompletedTasks,
                InProgressTasks = response.InProgressTasks,
                OverdueTasks = response.OverdueTasks,
                HighPriorityPendingTasks = response.HighPriorityPendingTasks,
                CompletionRate = response.CompletionRate,
                AverageCompletionTimeHours = response.AverageCompletionTimeHours,
                TasksCompletedToday = response.TasksCompletedThisToday,
                Notes = notes,
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            };
        }

        public static TaskReportResponse ToResponse(TaskProgressReport entity)
        {
            return new TaskReportResponse
            {
                TotalTasks = entity.TotalTasks,
                CompletedTasks = entity.CompletedTasks,
                InProgressTasks = entity.InProgressTasks,
                OverdueTasks = entity.OverdueTasks,
                HighPriorityPendingTasks = entity.HighPriorityPendingTasks,
                CompletionRate = entity.CompletionRate,
                AverageCompletionTimeHours = entity.AverageCompletionTimeHours,
                TasksCompletedThisToday = entity.TasksCompletedToday
            };
        }

        //public static DailyCompletionTrend CreateDailyTrend(DateTime date, List<Models.Entities.Task> allTasks)
        //{
        //    return new DailyCompletionTrend
        //    {
        //        Date = date,
        //        CompletedCount = allTasks.Count(t =>
        //            t.IsCompleted && t.CompletedOn.HasValue && t.CompletedOn.Value.Date == date),
        //        CreatedCount = allTasks.Count(t =>
        //            t.CreatedOn.HasValue && t.CreatedOn.Value.Date == date)
        //    };
        //}

        public static PriorityDistribution CreatePriorityDistribution(List<Models.Entities.Task> allTasks)
        {
            return new PriorityDistribution
            {
                HighPriority = allTasks.Count(t => t.Priority == Commons.Enums.Tier.High),
                MediumPriority = allTasks.Count(t => t.Priority == Commons.Enums.Tier.Medium),
                LowPriority = allTasks.Count(t => t.Priority == Commons.Enums.Tier.Low)
            };
        }
    }
}
