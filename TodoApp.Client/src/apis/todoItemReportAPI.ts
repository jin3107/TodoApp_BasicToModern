import axios from '../configs/axios'
import type { AppResponse } from "../helpers";
import type { TodoItemReportRequest, TodoItemReportResponse } from '../interfaces';

export const getProgressReport = async (request: TodoItemReportRequest) : Promise<AppResponse<TodoItemReportResponse>> => {
  const response = await axios.post<AppResponse<TodoItemReportResponse>>('/reports/progress', request, {
    timeout: 30000,
  });
  return response.data;
}

export const createDailySnapshot = async () : Promise<AppResponse<string>> => {
  const response = await axios.post<AppResponse<string>>('/reports/snapshot');
  return response.data;
}
