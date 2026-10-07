import dayjs from "dayjs";
import type { DailyCompletionTrendResponse } from "../interfaces/Responses";

export const buildTrailingWeekTrend = (trend: DailyCompletionTrendResponse[]) => {
  const countByDate = new Map(trend.map((t) => [dayjs(t.date).format("YYYY-MM-DD"), t.completedCount]));

  const days = [];
  for (let i = 6; i >= 0; i--) {
    const date = dayjs().subtract(i, "day");
    const key = date.format("YYYY-MM-DD");
    days.push({ label: date.format("DD/MM"), value: countByDate.get(key) ?? 0 });
  }
  return days;
};
