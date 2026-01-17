export default interface TodoItemReportItemResponse {
  id: string;
  title: string;
  description: string;
  dueDate: string;
  isCompleted: boolean;
  priority: number;
  completedOn?: string;
}