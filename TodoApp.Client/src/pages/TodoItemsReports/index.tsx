import { useState, useEffect, useCallback } from 'react';
import { App, Button } from 'antd';
import { ReloadOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import './style.scss';
import type { TodoItemReportResponse, TodoItemResponse } from '../../interfaces/Responses';
import { getProgressReport } from '../../apis/todoItemReportAPI';
import { PriorityTag, MiniLineChart } from '../../components';
import { buildTrailingWeekTrend } from '../../commons/trend';

const RANGE_OPTIONS = [7, 30, 90] as const;

const TasksReports = () => {
  const { message } = App.useApp();
  const [loading, setLoading] = useState(false);
  const [reportData, setReportData] = useState<TodoItemReportResponse | null>(null);
  const [rangeDays, setRangeDays] = useState<number>(30);

  const fetchReport = useCallback(async (days: number) => {
    try {
      setLoading(true);
      const request = {
        startDate: dayjs().subtract(days - 1, 'day').format('YYYY-MM-DD'),
        endDate: dayjs().format('YYYY-MM-DD'),
      };

      const response = await getProgressReport(request);

      if (response.isSuccess && response.data) {
        setReportData(response.data);
      } else {
        message.error(response.message || 'Không thể tải báo cáo');
      }
    } catch (error) {
      console.error('Error fetching report:', error);
      message.error('Có lỗi xảy ra khi tải báo cáo');
    } finally {
      setLoading(false);
    }
  }, [message]);

  useEffect(() => {
    fetchReport(rangeDays);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const changeRange = (days: number) => {
    setRangeDays(days);
    fetchReport(days);
  };

  const getDaysOverdue = (dueDate: string | dayjs.Dayjs) => {
    const days = dayjs().diff(dayjs(dueDate), 'day');
    return days > 0 ? days : 0;
  };

  if (!reportData) {
    return (
      <div className="loading-state">
        <span>Đang tải...</span>
      </div>
    );
  }

  const trendData = buildTrailingWeekTrend(reportData.completionTrend);

  const { highPriority, mediumPriority, lowPriority } = reportData.priorityDistribution;
  const totalPriority = Math.max(highPriority + mediumPriority + lowPriority, 1);
  const highPct = Math.round((highPriority / totalPriority) * 100);
  const medPct = Math.round((mediumPriority / totalPriority) * 100);
  const donutGradient = `conic-gradient(var(--color-accent-500) 0% ${highPct}%, var(--color-accent-300) ${highPct}% ${highPct + medPct}%, var(--color-neutral-300) ${highPct + medPct}% 100%)`;

  const maxProd = Math.max(
    reportData.tasksCompletedThisToday,
    reportData.tasksCompletedThisWeek,
    reportData.tasksCompletedThisMonth,
    1,
  );
  const barHeight = (val: number) => Math.max(4, Math.round((val / maxProd) * 80));

  const completionRate = reportData.totalTasks > 0
    ? Math.round((reportData.completedTasks / reportData.totalTasks) * 100)
    : 0;

  const overdueRow = (task: TodoItemResponse, index: number) => (
    <div className="reports-row" key={task.id ?? index}>
      <div className="reports-row-text">
        <span className="reports-row-title">{task.title}</span>
        <PriorityTag priority={task.priority} />
      </div>
      <span className="cls-tag cls-tag-outline">{getDaysOverdue(task.dueDate)} ngày</span>
    </div>
  );

  return (
    <div className="report-container">
      <div className="reports-header">
        <h1>Báo cáo tiến độ công việc</h1>
        <div className="reports-header-actions">
          <div className="cls-seg">
            {RANGE_OPTIONS.map((d) => (
              <button
                key={d}
                className={`cls-seg-opt ${rangeDays === d ? 'active' : ''}`}
                onClick={() => changeRange(d)}
              >
                {d} ngày
              </button>
            ))}
          </div>
          <Button type="primary" icon={<ReloadOutlined />} onClick={() => fetchReport(rangeDays)} loading={loading}>
            Làm mới
          </Button>
        </div>
      </div>

      <div className="reports-stats-grid">
        <div className="cls-card"><span className="cls-card-kicker">Tổng công việc</span><span className="cls-card-title">{reportData.totalTasks}</span></div>
        <div className="cls-card"><span className="cls-card-kicker">Đã hoàn thành</span><span className="cls-card-title">{reportData.completedTasks}</span></div>
        <div className="cls-card"><span className="cls-card-kicker">Đang thực hiện</span><span className="cls-card-title">{reportData.inProgressTasks}</span></div>
        <div className="cls-card"><span className="cls-card-kicker">Quá hạn</span><span className="cls-card-title reports-overdue-value">{reportData.overdueTasks}</span></div>
      </div>

      <div className="reports-stats-grid">
        <div className="cls-card"><span className="cls-card-kicker">Ưu tiên cao chưa xong</span><span className="cls-card-title reports-value-sm">{reportData.highPriorityPendingTasks}</span></div>
        <div className="cls-card"><span className="cls-card-kicker">Ưu tiên trung bình</span><span className="cls-card-title reports-value-sm">{reportData.mediumPriorityPendingTasks}</span></div>
        <div className="cls-card"><span className="cls-card-kicker">Ưu tiên thấp</span><span className="cls-card-title reports-value-sm">{reportData.lowPriorityPendingTasks}</span></div>
      </div>

      <div className="reports-two-col">
        <div className="cls-card">
          <h3>Xu hướng hoàn thành</h3>
          {trendData.some((d) => d.value > 0) ? <MiniLineChart data={trendData} height={150} /> : <p className="text-muted">Chưa có dữ liệu</p>}
        </div>

        <div className="cls-card reports-donut-card">
          <h3>Phân bổ theo độ ưu tiên</h3>
          <div className="reports-donut" style={{ background: donutGradient }} />
          <div className="reports-legend">
            <span><i className="reports-legend-swatch" style={{ background: 'var(--color-accent-500)' }} />Cao</span>
            <span><i className="reports-legend-swatch" style={{ background: 'var(--color-accent-300)' }} />Trung bình</span>
            <span><i className="reports-legend-swatch" style={{ background: 'var(--color-neutral-300)' }} />Thấp</span>
          </div>
        </div>
      </div>

      <div className="cls-card">
        <h3>Năng suất hoàn thành</h3>
        <div className="reports-bar-chart">
          <div className="reports-bar-item">
            <span className="reports-bar-value">{reportData.tasksCompletedThisToday}</span>
            <div className="reports-bar" style={{ height: barHeight(reportData.tasksCompletedThisToday) }} />
            <span className="text-muted">Hôm nay</span>
          </div>
          <div className="reports-bar-item">
            <span className="reports-bar-value">{reportData.tasksCompletedThisWeek}</span>
            <div className="reports-bar" style={{ height: barHeight(reportData.tasksCompletedThisWeek) }} />
            <span className="text-muted">Tuần này</span>
          </div>
          <div className="reports-bar-item">
            <span className="reports-bar-value">{reportData.tasksCompletedThisMonth}</span>
            <div className="reports-bar" style={{ height: barHeight(reportData.tasksCompletedThisMonth) }} />
            <span className="text-muted">Tháng này</span>
          </div>
        </div>
      </div>

      <div className="reports-two-col">
        <div className="cls-card">
          <h3>Sắp đến hạn (3 ngày tới)</h3>
          <p className="text-muted reports-empty-note">Không có công việc nào sắp đến hạn.</p>
        </div>

        <div className="cls-card">
          <h3>Top 5 quá hạn lâu nhất</h3>
          {reportData.mostOverdueTasks.length > 0 ? (
            <div className="reports-list">
              {reportData.mostOverdueTasks.slice(0, 5).map(overdueRow)}
            </div>
          ) : (
            <p className="text-muted reports-empty-note">Không có công việc quá hạn.</p>
          )}
        </div>
      </div>

      <div className="cls-card">
        <h3>Thời gian hoàn thành trung bình</h3>
        <div className="reports-avg-time">
          <span className="cls-card-title">{reportData.averageCompletionTimeHours.toFixed(1)} giờ</span>
          <span className="text-muted">Tỷ lệ hoàn thành: {completionRate}%</span>
        </div>
      </div>
    </div>
  );
};

export default TasksReports;
