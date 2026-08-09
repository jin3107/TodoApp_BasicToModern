import dayjs from "dayjs";
import type { Tier } from "../../commons/enums/Tier";

export default interface TodoItemResponse {
  id?: string,
  title: string;
  description: string;
  dueDate: dayjs.Dayjs;
  isCompleted: boolean;
  priority: Tier;
  completedOn: dayjs.Dayjs;
  todoListId?: string;

  createdOn?: dayjs.Dayjs;
  modifiedOn?: dayjs.Dayjs;
}
