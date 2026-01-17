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
import type { DailyCompletionTrendResponse, TodoItemReportItemResponse, TodoItemReportResponse } from '../../interfaces/Responses';
import { getProgressReport } from '../../apis/todoItemReportAPI';
import { StatsCard, PriorityTag, useDateFormatter } from '../../components';

const { Title, Text } = Typography;
const { RangePicker } = DatePicker;

const TasksReports = () => {
  const { formatDate } = useDateFormatter();
  const [loading, setLoading] = useState(false);
  const [reportData, setReportData] = useState<TodoItemReportResponse | null>(null);
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs, dayjs.Dayjs] | null>(null);

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
  }, []);

  const getDaysOverdue = (dueDate: string) => {
    const days = dayjs().diff(dayjs(dueDate), 'day');
    return days > 0 ? days : 0;
  };

  const upcomingColumns = [
    {
      title: 'Tiêu đề',
      dataIndex: 'title',
      key: 'title',
      width: '40%',
      render: (text: string, record: TodoItemReportItemResponse) => (
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
      render: (priority: number) => <PriorityTag priority={priority} />,
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
      render: (_: unknown, record: TodoItemReportItemResponse) => {
        const days = dayjs(record.dueDate).diff(dayjs(), 'day');
        const color = days <= 1 ? 'red' : days <= 3 ? 'orange' : 'green';
        return <Tag color={color}>{days} ngày</Tag>;
      },
    },
  ];

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
      render: (priority: number) => <PriorityTag priority={priority} />,
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
      render: (_: unknown, record: TodoItemReportItemResponse) => (
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

  const pieChartData = reportData.priorityDistribution
    ? [
        { type: 'Cao', value: reportData.priorityDistribution.highPriority },
        { type: 'Trung bình', value: reportData.priorityDistribution.mediumPriority },
        { type: 'Thấp', value: reportData.priorityDistribution.lowPriority },
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
        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
          <Col xs={24} sm={12} md={6}>
            <StatsCard
              title="Tổng số công việc"
              value={reportData.totalTasks}
              prefix={<TrophyOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <StatsCard
              title="Đã hoàn thành"
              value={reportData.completedTasks}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#52c41a' }}
              showProgress
              progressPercent={completionRate}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <StatsCard
              title="Đang thực hiện"
              value={reportData.inProgressTasks}
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <StatsCard
              title="Quá hạn"
              value={reportData.overdueTasks}
              prefix={<WarningOutlined />}
              valueStyle={{ color: '#ff4d4f' }}
            />
          </Col>
        </Row>

        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
          <Col xs={24} sm={8}>
            <StatsCard
              title="Ưu tiên cao (chưa hoàn thành)"
              value={reportData.highPriorityPendingTasks}
              prefix={<ExclamationCircleOutlined />}
              valueStyle={{ color: '#ff4d4f' }}
            />
          </Col>
          <Col xs={24} sm={8}>
            <StatsCard
              title="Ưu tiên trung bình"
              value={reportData.mediumPriorityPendingTasks}
              prefix={<RiseOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Col>
          <Col xs={24} sm={8}>
            <StatsCard
              title="Ưu tiên thấp"
              value={reportData.lowPriorityPendingTasks}
              prefix={<FallOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Col>
        </Row>

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
                dataSource={[]}
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
                dataSource={reportData.mostOverdueTasks.map(task => ({
                  id: task.id || '',
                  title: task.title,
                  description: task.description,
                  dueDate: typeof task.dueDate === 'string' ? task.dueDate : task.dueDate.toISOString(),
                  isCompleted: task.isCompleted,
                  priority: task.priority,
                  completedOn: task.completedOn ? (typeof task.completedOn === 'string' ? task.completedOn : task.completedOn.toISOString()) : undefined,
                }))}
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