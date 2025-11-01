export default interface TaskReportItemResponse {
  id: string;
  title: string;
  description: string;
  dueDate: string;
  isCompleted: boolean;
  priority: number;
  completedOn?: string;
}