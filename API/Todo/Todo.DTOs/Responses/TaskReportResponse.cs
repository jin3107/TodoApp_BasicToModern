using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.DTOs.Responses
{
    public class TaskReportResponse
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int HighPriorityPendingTasks { get; set; }
        public int MediumPriorityPendingTasks { get; set; }
        public int LowPriorityPendingTasks { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageCompletionTimeHours { get; set; }
        public int TasksCompletedThisToday { get; set; }
        public int TasksCompletedThisWeek { get; set; }
        public int TasksCompletedThisMonth { get; set; }

        public List<TaskResponse> MostOverdueTasks { get; set; }
        public PriorityDistribution PriorityDistribution { get; set; }
        public List<DailyCompletionTrend> CompletionTrend { get; set; }
    }

    public class PriorityDistribution
    {
        public int HighPriority { get; set; }
        public int MediumPriority { get; set; }
        public int LowPriority { get; set; }
    }

    public class DailyCompletionTrend
    {
        public DateTime Date { get; set; }
        public int CompletedCount { get; set; }
        public int CreatedCount { get; set; }
    }
}
