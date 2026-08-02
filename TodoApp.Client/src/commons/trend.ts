import dayjs from "dayjs";
import type { DailyCompletionTrendResponse } from "../interfaces/Responses";

/**
 * The Classical design's completion-trend chart always plots the trailing 7
 * calendar days (one evenly-spaced point per day, zero-filled), independent
 * of whatever stat range (7/30/90) is selected elsewhere on the page —
 * mirrors the trendDays computation in the design handoff's prototype
 * script. Building it this way (instead of mapping the API's completionTrend
 * array directly) guarantees the chart is always chronologically sorted and
 * evenly spaced, so dots/line/date-labels never end up misaligned.
 */
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
