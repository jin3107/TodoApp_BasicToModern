import axios from '../configs/axios'
import type { AppResponse } from "../helpers";
import type { TaskReportRequest } from '../interfaces/Requests';
import type { TaskReportResponse } from '../interfaces/Responses';

export const getProgressReport = async (request: TaskReportRequest) : Promise<AppResponse<TaskReportResponse>> => {
  const response = await axios.post<AppResponse<TaskReportResponse>>('/reports/progress',request);
  return response.data;
}

export const createDailySnapshot = async () : Promise<AppResponse<string>> => {
  const response = await axios.post<AppResponse<string>>('/reports/snapshot');
  return response.data;
}