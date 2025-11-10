using MayNghien.Infrastructures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.Models.Entities
{
    public class TodoItemProgressReport : BaseEntity
    {
        public DateTime ReportDate { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int HighPriorityPendingTasks { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageCompletionTimeHours { get; set; }
        public int TasksCompletedToday { get; set; }
        public string Notes { get; set; }
    }
}
