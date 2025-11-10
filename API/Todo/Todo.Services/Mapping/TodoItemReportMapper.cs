using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Responses;
using Todo.Models.Entities;

namespace Todo.Services.Mapping
{
    public static class TodoItemReportMapper
    {
        public static TodoItemProgressReport ToEntity(TodoItemReportResponse response, DateTime reportDate, string notes = "")
        {
            return new TodoItemProgressReport
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

        public static TodoItemReportResponse ToResponse(TodoItemProgressReport entity)
        {
            return new TodoItemReportResponse
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
    }
}
