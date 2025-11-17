import type dayjs from "dayjs";
import type { Tier } from "../../commons";

export default interface TodoItemRequest {
  title: string;
  description: string;
  dueDate: dayjs.Dayjs;
  isCompleted: boolean;
  priority: Tier;
  completedOn: dayjs.Dayjs;
}