import type DailyCompletionTrendResponse from "./DailyCompletionTrendResponse";
import type TaskResponse from "./TaskResponse";

export default interface TaskReportResponse {
  totalTasks: number;
  completedTasks: number;
  inProgressTasks: number;
  overdueTasks: number;
  highPriorityPendingTasks: number;
  mediumPriorityPendingTasks: number;
  lowPriorityPendingTasks: number;
  completionRate: number;
  averageCompletionTimeHours: number;
  tasksCompletedThisToday: number;
  tasksCompletedThisWeek: number;
  tasksCompletedThisMonth: number;
  mostOverdueTasks: TaskResponse[];
  priorityDistribution: PriorityDistribution;
  completionTrend: DailyCompletionTrendResponse[];
}

export interface PriorityDistribution {
  highPriority: number;
  mediumPriority: number;
  lowPriority: number;
}