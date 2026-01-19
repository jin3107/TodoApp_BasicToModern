import dayjs from "dayjs";
import type { Tier } from "../../commons";

export default interface TodoItemResponse {
  id?: string,
  title: string;
  description: string;
  dueDate: dayjs.Dayjs;
  isCompleted: boolean;
  priority: Tier;
  completedOn: dayjs.Dayjs;
  todoListId?: string; // ✅ Add TodoListId property

  createdOn?: dayjs.Dayjs;
  // createdBy?: string;
  modifiedOn?: dayjs.Dayjs;
}