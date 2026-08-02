import { useState, useEffect, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { App, Button, Tag } from 'antd';
import {
  ArrowRightOutlined,
  WarningOutlined,
} from '@ant-design/icons';
import dayjs, { Dayjs } from 'dayjs';
import './style.scss';
import type { TodoItemReportResponse } from '../../interfaces/Responses';
import { getProgressReport } from '../../apis/todoItemReportAPI';
import { PriorityTag, MiniLineChart } from '../../components';
import { buildTrailingWeekTrend } from '../../commons/trend';

const RANGE_OPTIONS = [7, 30, 90] as const;

const Dashboard = () => {
  const navigate = useNavigate();
  const { message } = App.useApp();
  const didLoadInitialData = useRef(false);
  const [loading, setLoading] = useState(false);
  const [reportData, setReportData] = useState<TodoItemReportResponse | null>(null);
  const [rangeDays, setRangeDays] = useState<number>(30);

  const fetchDashboardData = useCallback(async (days: number) => {
    try {
      setLoading(true);
      const endDate: Dayjs = dayjs();
      const startDate: Dayjs = dayjs().subtract(days - 1, 'day');
      const progressResponse = await getProgressReport({
        startDate: startDate.format('YYYY-MM-DD'),
        endDate: endDate.format('YYYY-MM-DD'),
      });
      if (progressResponse.isSuccess && progressResponse.data) {
        setReportData(progressResponse.data);
      } else {
        message.error(progressResponse.message || 'Không thể tải dashboard');
      }
    } catch (error) {
      console.error('Error fetching dashboard:', error);
      message.error('Có lỗi xảy ra khi tải dashboard');
    } finally {
      setLoading(false);
    }
  }, [message]);

  useEffect(() => {
    if (didLoadInitialData.current) return;
    didLoadInitialData.current = true;
    fetchDashboardData(rangeDays);
  }, [rangeDays, fetchDashboardData]);

  const changeRange = (days: number) => {
    setRangeDays(days);
    fetchDashboardData(days);
  };

  if (loading && !reportData) {
    return (
      <div className="loading-state dashboard-loading">
        <span>Đang tải...</span>
      </div>
    );
  }

  const completionRate = reportData
    ? Math.round((reportData.completedTasks / Math.max(reportData.totalTasks, 1)) * 100)
    : 0;

  const trendData = buildTrailingWeekTrend(reportData?.completionTrend || []);

  return (
    <div className="dashboard-container page-shell">
      <div className="dashboard-header">
        <div>
          <h1>Trang chủ</h1>
          <p className="text-muted">Đây là tổng quan về công việc của bạn.</p>
        </div>
        <Button type="primary" icon={<ArrowRightOutlined />} onClick={() => navigate('/todo-lists')}>
          Xem tất cả công việc
        </Button>
      </div>

      <div className="dashboard-range">
        <span className="text-muted">Khoảng thời gian:</span>
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
      </div>

      <div className="dashboard-stats-grid">
        <div className="cls-card">
          <span className="cls-card-kicker">Tổng công việc</span>
          <span className="cls-card-title">{reportData?.totalTasks ?? 0}</span>
        </div>
        <div className="cls-card">
          <span className="cls-card-kicker">Đã hoàn thành</span>
          <span className="cls-card-title">{reportData?.completedTasks ?? 0}</span>
          <span className="text-muted dashboard-stat-sub">{completionRate}% tỷ lệ hoàn thành</span>
        </div>
        <div className="cls-card">
          <span className="cls-card-kicker">Đang thực hiện</span>
          <span className="cls-card-title">{reportData?.inProgressTasks ?? 0}</span>
        </div>
        <div className="cls-card">
          <span className="cls-card-kicker">Quá hạn</span>
          <span className="cls-card-title dashboard-overdue-value">{reportData?.overdueTasks ?? 0}</span>
        </div>
      </div>

      <div className="cls-card dashboard-chart-card">
        <div className="dashboard-card-header">
          <h3>Biểu đồ hoàn thành</h3>
          <Tag>{rangeDays} ngày gần nhất</Tag>
        </div>
        {trendData.some((d) => d.value > 0) ? (
          <MiniLineChart data={trendData} height={160} />
        ) : (
          <p className="text-muted">Chưa có dữ liệu xu hướng</p>
        )}
      </div>

      <div className="dashboard-two-col">
        <div className="cls-card">
          <h3>Năng suất của bạn</h3>
          <div className="dashboard-productivity-row">
            <div className="dashboard-productivity-item">
              <span className="text-muted">Hôm nay</span>
              <div className="dashboard-productivity-value">{reportData?.tasksCompletedThisToday ?? 0}</div>
            </div>
            <div className="dashboard-productivity-item">
              <span className="text-muted">Tuần này</span>
              <div className="dashboard-productivity-value">{reportData?.tasksCompletedThisWeek ?? 0}</div>
            </div>
            <div className="dashboard-productivity-item">
              <span className="text-muted">Tháng này</span>
              <div className="dashboard-productivity-value">{reportData?.tasksCompletedThisMonth ?? 0}</div>
            </div>
          </div>
          <div className="dashboard-summary-box">
            <div className="dashboard-summary-row">
              <span>Thời gian hoàn thành TB</span>
              <strong>{reportData?.averageCompletionTimeHours?.toFixed(1) ?? 0} giờ</strong>
            </div>
            <div className="dashboard-summary-row">
              <span>Tỷ lệ hoàn thành</span>
              <strong>{completionRate}%</strong>
            </div>
          </div>
        </div>

        <div className="cls-card">
          <h3>Cần chú ý</h3>
          <div className="dashboard-productivity-row">
            <div className="dashboard-productivity-item">
              <span className="text-muted">Ưu tiên cao</span>
              <div className="dashboard-productivity-value dashboard-overdue-value">
                {reportData?.highPriorityPendingTasks ?? 0}
              </div>
            </div>
            <div className="dashboard-productivity-item">
              <span className="text-muted">Ưu tiên TB</span>
              <div className="dashboard-productivity-value">{reportData?.mediumPriorityPendingTasks ?? 0}</div>
            </div>
            <div className="dashboard-productivity-item">
              <span className="text-muted">Ưu tiên thấp</span>
              <div className="dashboard-productivity-value">{reportData?.lowPriorityPendingTasks ?? 0}</div>
            </div>
          </div>
          {(reportData?.overdueTasks ?? 0) > 0 && (
            <div className="dashboard-alert">
              <WarningOutlined />
              Bạn có {reportData?.overdueTasks} công việc quá hạn!
            </div>
          )}
        </div>
      </div>

      <div className="cls-card">
        <div className="dashboard-card-header">
          <h3>Quá hạn</h3>
          <a href="#" onClick={(e) => { e.preventDefault(); navigate('/todo-lists'); }}>
            Xem tất cả →
          </a>
        </div>
        {reportData?.mostOverdueTasks && reportData.mostOverdueTasks.length > 0 ? (
          <div className="dashboard-overdue-list">
            {reportData.mostOverdueTasks.slice(0, 5).map((item) => {
              const dueDateDayjs = typeof item.dueDate === 'string' ? dayjs(item.dueDate) : item.dueDate;
              const daysOverdue = dayjs().diff(dueDateDayjs, 'day');
              return (
                <div key={item.id} className="dashboard-overdue-row">
                  <div className="dashboard-overdue-row-text">
                    <span className="dashboard-overdue-row-title">{item.title}</span>
                    <PriorityTag priority={item.priority} />
                  </div>
                  <span className="cls-tag cls-tag-outline">{daysOverdue} ngày</span>
                </div>
              );
            })}
          </div>
        ) : (
          <p className="text-muted dashboard-empty-note">Tuyệt vời! Không có công việc quá hạn.</p>
        )}
      </div>

      <div className="cls-card">
        <h3>Hành động nhanh</h3>
        <div className="dashboard-quick-actions">
          <Button type="primary" onClick={() => navigate('/todo-lists')}>
            Quản lý công việc
          </Button>
          <Button onClick={() => fetchDashboardData(rangeDays)} loading={loading}>
            Làm mới dữ liệu
          </Button>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
