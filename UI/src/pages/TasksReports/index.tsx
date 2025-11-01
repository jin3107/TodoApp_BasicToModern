import { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Statistic,
  Progress,
  Table,
  Tag,
  Typography,
  Spin,
  message,
  Space,
  DatePicker,
  Button,
} from 'antd';
import {
  CheckCircleOutlined,
  ClockCircleOutlined,
  ExclamationCircleOutlined,
  RiseOutlined,
  FallOutlined,
  TrophyOutlined,
  WarningOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { Line, Bar, Pie } from '@ant-design/charts';
import './style.scss';
import type { DailyCompletionTrendResponse, TaskReportItemResponse, TaskReportResponse } from '../../interfaces/Responses';
import { getProgressReport } from '../../apis/taskReportAPI';

const { Title, Text } = Typography;
const { RangePicker } = DatePicker;

/**
 * Component TasksReports - Hiển thị báo cáo và thống kê tiến độ công việc
 * 
 * Business Logic tổng quan:
 * 1. Hiển thị các metrics chính: tổng task, completion rate, overdue, priority distribution
 * 2. Biểu đồ xu hướng hoàn thành theo thời gian (7 ngày gần nhất)
 * 3. Danh sách tasks cần ưu tiên: sắp đến hạn, quá hạn lâu nhất
 * 4. Thống kê theo độ ưu tiên (Pie chart)
 * 5. Cho phép filter báo cáo theo khoảng thời gian
 */

const TasksReports = () => {
  const [loading, setLoading] = useState(false);
  const [reportData, setReportData] = useState<TaskReportResponse | null>(null);
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs, dayjs.Dayjs] | null>(null);

  /**
   * Fetch báo cáo từ API
   * Business Logic: Gọi API với filter theo dateRange nếu có
   */
  const fetchReport = async () => {
    try {
      setLoading(true);
      const request = dateRange
        ? {
            startDate: dateRange[0].format('YYYY-MM-DD'),
            endDate: dateRange[1].format('YYYY-MM-DD'),
          }
        : {};

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
  };

  useEffect(() => {
    fetchReport();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  /**
   * Render tag độ ưu tiên
   */
  const getPriorityTag = (priority: number) => {
    switch (priority) {
      case 2:
        return <Tag color="red">Cao</Tag>;
      case 1:
        return <Tag color="orange">Trung bình</Tag>;
      case 0:
        return <Tag color="green">Thấp</Tag>;
      default:
        return <Tag>Không xác định</Tag>;
    }
  };

  /**
   * Format ngày tháng
   */
  const formatDate = (dateStr?: string) => {
    if (!dateStr) return '-';
    return dayjs(dateStr).format('DD/MM/YYYY');
  };

  /**
   * Tính số ngày quá hạn
   * Business Logic: Hiển thị task quá hạn bao nhiêu ngày
   */
  const getDaysOverdue = (dueDate: string) => {
    const days = dayjs().diff(dayjs(dueDate), 'day');
    return days > 0 ? days : 0;
  };

  // Columns cho table "Tasks sắp đến hạn"

  // Columns cho table "Tasks sắp đến hạn"
  const upcomingColumns = [
    {
      title: 'Tiêu đề',
      dataIndex: 'title',
      key: 'title',
      width: '40%',
      render: (text: string, record: TaskReportItemResponse) => (
        <Space>
          {record.priority === 2 && <ExclamationCircleOutlined style={{ color: 'red' }} />}
          <Text strong>{text}</Text>
        </Space>
      ),
    },
    {
      title: 'Độ ưu tiên',
      dataIndex: 'priority',
      key: 'priority',
      width: '20%',
      render: (priority: number) => getPriorityTag(priority),
    },
    {
      title: 'Hạn hoàn thành',
      dataIndex: 'dueDate',
      key: 'dueDate',
      width: '20%',
      render: (date: string) => formatDate(date),
    },
    {
      title: 'Còn lại',
      key: 'remaining',
      width: '20%',
      render: (_: unknown, record: TaskReportItemResponse) => {
        const days = dayjs(record.dueDate).diff(dayjs(), 'day');
        const color = days <= 1 ? 'red' : days <= 3 ? 'orange' : 'green';
        return <Tag color={color}>{days} ngày</Tag>;
      },
    },
  ];

  // Columns cho table "Tasks quá hạn"

  // Columns cho table "Tasks quá hạn"
  const overdueColumns = [
    {
      title: 'Tiêu đề',
      dataIndex: 'title',
      key: 'title',
      width: '40%',
      render: (text: string) => (
        <Space>
          <WarningOutlined style={{ color: 'red' }} />
          <Text strong>{text}</Text>
        </Space>
      ),
    },
    {
      title: 'Độ ưu tiên',
      dataIndex: 'priority',
      key: 'priority',
      width: '20%',
      render: (priority: number) => getPriorityTag(priority),
    },
    {
      title: 'Hạn hoàn thành',
      dataIndex: 'dueDate',
      key: 'dueDate',
      width: '20%',
      render: (date: string) => formatDate(date),
    },
    {
      title: 'Quá hạn',
      key: 'overdue',
      width: '20%',
      render: (_: unknown, record: TaskReportItemResponse) => (
        <Tag color="red">{getDaysOverdue(record.dueDate)} ngày</Tag>
      ),
    },
  ];

  if (!reportData) {
    return (
      <div style={{ textAlign: 'center', padding: '100px' }}>
        <Spin size="large" />
      </div>
    );
  }

  /**
   * Cấu hình biểu đồ Line Chart - Xu hướng hoàn thành
   * Business Logic: Hiển thị số task completed theo ngày (7 ngày gần nhất)
   */
  const lineChartData = reportData.completionTrend.map((item: DailyCompletionTrendResponse) => ({
    date: dayjs(item.date).format('DD/MM'),
    value: item.completedCount,
  }));

  const lineConfig = {
    data: lineChartData,
    xField: 'date',
    yField: 'value',
    smooth: true,
    color: '#1890ff',
    point: {
      size: 4,
      shape: 'circle',
    },
    animation: {
      appear: {
        animation: 'path-in',
        duration: 1000,
      },
    },
  };

  /**
   * Cấu hình Pie Chart - Phân bổ theo độ ưu tiên
   * Business Logic: Hiển thị % tasks theo mức độ ưu tiên
   */
  const pieChartData = reportData.priorityDistribution.length > 0
    ? [
        { type: 'Cao', value: reportData.priorityDistribution[0].highPriority },
        { type: 'Trung bình', value: reportData.priorityDistribution[1].mediumPriority },
        { type: 'Thấp', value: reportData.priorityDistribution[2].lowPriority },
      ]
    : [];

  const pieConfig = {
    data: pieChartData,
    angleField: 'value',
    colorField: 'type',
    radius: 0.8,
    label: {
      type: 'outer' as const,
      content: '{name} {percentage}',
    },
    color: ({ type }: { type: string }) => {
      if (type === 'Cao') return '#ff4d4f';
      if (type === 'Trung bình') return '#faad14';
      if (type === 'Thấp') return '#52c41a';
      return '#1890ff';
    },
    interactions: [
      { type: 'pie-legend-active' },
      { type: 'element-active' },
    ],
  };

  /**
   * Cấu hình Bar Chart - So sánh hoàn thành theo thời gian
   * Business Logic: Hiển thị productivity (số task hoàn thành) theo ngày/tuần/tháng
   */
  const barChartData = [
    { period: 'Hôm nay', count: reportData.tasksCompletedThisToday },
    { period: 'Tuần này', count: reportData.tasksCompletedThisWeek },
    { period: 'Tháng này', count: reportData.tasksCompletedThisMonth },
  ];

  const barConfig = {
    data: barChartData,
    xField: 'period',
    yField: 'count',
    label: {
      position: 'top' as const,
      style: {
        fill: '#000000',
        opacity: 0.6,
      },
    },
    color: '#1890ff',
    meta: {
      period: { alias: 'Thời gian' },
      count: { alias: 'Số lượng' },
    },
  };

  const completionRate = reportData.totalTasks > 0 
    ? Math.round((reportData.completedTasks / reportData.totalTasks) * 100) 
    : 0;

  return (
    <div className="report-container">
      <div className="report-header">
        <Title level={2}>Báo cáo tiến độ công việc</Title>
        <Space>
          <RangePicker
            value={dateRange}
            onChange={(dates) => setDateRange(dates as [dayjs.Dayjs, dayjs.Dayjs])}
            format="DD/MM/YYYY"
          />
          <Button type="primary" icon={<ReloadOutlined />} onClick={fetchReport} loading={loading}>
            Làm mới
          </Button>
        </Space>
      </div>

      <Spin spinning={loading}>
        {/* Business Logic 1: Các chỉ số tổng quan chính */}
        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="Tổng số công việc"
                value={reportData.totalTasks}
                prefix={<TrophyOutlined />}
                valueStyle={{ color: '#1890ff' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="Đã hoàn thành"
                value={reportData.completedTasks}
                prefix={<CheckCircleOutlined />}
                valueStyle={{ color: '#52c41a' }}
              />
              <Progress
                percent={completionRate}
                size="small"
                status="active"
                format={(percent) => `${percent}%`}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="Đang thực hiện"
                value={reportData.inProgressTasks}
                prefix={<ClockCircleOutlined />}
                valueStyle={{ color: '#faad14' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="Quá hạn"
                value={reportData.overdueTasks}
                prefix={<WarningOutlined />}
                valueStyle={{ color: '#ff4d4f' }}
              />
            </Card>
          </Col>
        </Row>

        {/* Business Logic 2: Thống kê theo độ ưu tiên */}
        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
          <Col xs={24} sm={8}>
            <Card>
              <Statistic
                title="Ưu tiên cao (chưa hoàn thành)"
                value={reportData.highPriorityPendingTasks}
                prefix={<ExclamationCircleOutlined />}
                valueStyle={{ color: '#ff4d4f' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={8}>
            <Card>
              <Statistic
                title="Ưu tiên trung bình"
                value={reportData.mediumPriorityPendingTasks}
                prefix={<RiseOutlined />}
                valueStyle={{ color: '#faad14' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={8}>
            <Card>
              <Statistic
                title="Ưu tiên thấp"
                value={reportData.lowPriorityPendingTasks}
                prefix={<FallOutlined />}
                valueStyle={{ color: '#52c41a' }}
              />
            </Card>
          </Col>
        </Row>

        {/* Business Logic 3: Metrics về thời gian và productivity */}
        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
          <Col xs={24} sm={12}>
            <Card title="Thời gian hoàn thành trung bình">
              <Statistic
                value={reportData.averageCompletionTimeHours}
                suffix="giờ"
                precision={1}
                valueStyle={{ color: '#1890ff' }}
              />
              <Text type="secondary">
                Thời gian trung bình từ lúc tạo đến khi hoàn thành task
              </Text>
            </Card>
          </Col>
          <Col xs={24} sm={12}>
            <Card title="Năng suất hoàn thành">
              <Space direction="vertical" style={{ width: '100%' }}>
                <div>
                  <Text strong>Hôm nay: </Text>
                  <Tag color="blue">{reportData.tasksCompletedThisToday} tasks</Tag>
                </div>
                <div>
                  <Text strong>Tuần này: </Text>
                  <Tag color="cyan">{reportData.tasksCompletedThisWeek} tasks</Tag>
                </div>
                <div>
                  <Text strong>Tháng này: </Text>
                  <Tag color="geekblue">{reportData.tasksCompletedThisMonth} tasks</Tag>
                </div>
              </Space>
            </Card>
          </Col>
        </Row>

        {/* Business Logic 4: Biểu đồ phân tích */}
        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
          <Col xs={24} lg={12}>
            <Card title="Xu hướng hoàn thành 7 ngày gần nhất">
              <Line {...lineConfig} />
            </Card>
          </Col>
          <Col xs={24} lg={12}>
            <Card title="Phân bổ theo độ ưu tiên">
              <Pie {...pieConfig} />
            </Card>
          </Col>
        </Row>

        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
          <Col xs={24}>
            <Card title="Năng suất hoàn thành">
              <Bar {...barConfig} />
            </Card>
          </Col>
        </Row>

        {/* Business Logic 5: Danh sách tasks cần chú ý */}
        <Row gutter={[16, 16]}>
          <Col xs={24} lg={12}>
            <Card
              title={
                <Space>
                  <ClockCircleOutlined style={{ color: '#faad14' }} />
                  <span>Tasks sắp đến hạn (3 ngày tới)</span>
                </Space>
              }
            >
              <Table
                dataSource={reportData.upcomingTasks}
                columns={upcomingColumns}
                rowKey="id"
                pagination={false}
                size="small"
                locale={{ emptyText: 'Không có task nào sắp đến hạn' }}
              />
            </Card>
          </Col>
          <Col xs={24} lg={12}>
            <Card
              title={
                <Space>
                  <WarningOutlined style={{ color: '#ff4d4f' }} />
                  <span>Top 5 tasks quá hạn lâu nhất</span>
                </Space>
              }
            >
              <Table
                dataSource={reportData.mostOverdueTasks}
                columns={overdueColumns}
                rowKey="id"
                pagination={false}
                size="small"
                locale={{ emptyText: 'Không có task nào quá hạn' }}
              />
            </Card>
          </Col>
        </Row>
      </Spin>
    </div>
  );
};

export default TasksReports;