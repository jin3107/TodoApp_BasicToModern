import type DailyCompletionTrendResponse from "./DailyCompletionTrendResponse";
import type TodoItemResponse from "./TodoItemResponse";

export default interface TodoItemReportResponse {
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
  mostOverdueTasks: TodoItemResponse[];
  priorityDistribution: PriorityDistribution;
  completionTrend: DailyCompletionTrendResponse[];
}

export interface PriorityDistribution {
  highPriority: number;
  mediumPriority: number;
  lowPriority: number;
}