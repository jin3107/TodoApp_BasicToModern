import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Card,
  Row,
  Col,
  Typography,
  List,
  Tag,
  Button,
  Space,
  Empty,
  Spin,
  message,
  DatePicker,
} from 'antd';
import {
  CheckCircleOutlined,
  ClockCircleOutlined,
  ExclamationCircleOutlined,
  WarningOutlined,
  ArrowRightOutlined,
  TrophyOutlined,
  RocketOutlined,
  LineChartOutlined,
  CalendarOutlined,
} from '@ant-design/icons';
import { Line } from '@ant-design/charts';
import dayjs, { Dayjs } from 'dayjs';
import './style.scss';
import type { TodoItemReportResponse } from '../../interfaces/Responses';
import { getProgressReport } from '../../apis/todoItemReportAPI';
import { PageHeader, StatsCard, PriorityTag } from '../../components';

const { Text } = Typography;
const { RangePicker } = DatePicker;

const Dashboard = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [reportData, setReportData] = useState<TodoItemReportResponse | null>(null);
  const [dateRange, setDateRange] = useState<[Dayjs, Dayjs]>([
    dayjs().subtract(29, 'day'),
    dayjs()
  ]);

  const fetchDashboardData = useCallback(async () => {
    try {
      setLoading(true);
      const progressResponse = await getProgressReport({
        startDate: dateRange[0].format('YYYY-MM-DD'),
        endDate: dateRange[1].format('YYYY-MM-DD'),
      });
      if (progressResponse.isSuccess && progressResponse.data) {
        setReportData(progressResponse.data);
      }
    } catch (error) {
      console.error('Error fetching dashboard:', error);
      message.error('Có lỗi xảy ra khi tải dashboard');
    } finally {
      setLoading(false);
    }
  }, [dateRange]);

  useEffect(() => {
    fetchDashboardData();
  }, [fetchDashboardData]);

  const handleDateChange = (dates: null | [Dayjs | null, Dayjs | null]) => {
    if (dates && dates[0] && dates[1]) {
      setDateRange([dates[0], dates[1]]);
    }
  };

  const applyFilters = () => {
    fetchDashboardData();
  };

  if (loading && !reportData) {
    return (
      <div style={{ textAlign: 'center', padding: '100px' }}>
        <Spin size="large" />
      </div>
    );
  }

  const completionRate = reportData 
    ? Math.round((reportData.completedTasks / Math.max(reportData.totalTasks, 1)) * 100)
    : 0;

  return (
    <div className="dashboard-container">
      <PageHeader
        title="Dashboard"
        greeting
        subtitle="Đây là tổng quan về công việc của bạn"
        actions={
          <Button 
            type="primary" 
            icon={<ArrowRightOutlined />}
            onClick={() => navigate('/')}
            size="large"
          >
            Xem tất cả công việc
          </Button>
        }
      />

      <Card 
        style={{ marginBottom: 24 }}
        title={
          <Space>
            <CalendarOutlined style={{ color: '#1890ff' }} />
            <span>Bộ lọc thời gian</span>
          </Space>
        }
      >
        <Row gutter={[16, 16]} align="middle">
          <Col xs={24} md={20}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Chọn khoảng thời gian xem báo cáo:</Text>
              <RangePicker
                value={dateRange}
                onChange={handleDateChange}
                format="DD/MM/YYYY"
                style={{ width: '100%' }}
                placeholder={['Từ ngày', 'Đến ngày']}
              />
            </Space>
          </Col>
          <Col xs={24} md={4}>
            <Button 
              type="primary" 
              block 
              onClick={applyFilters}
              loading={loading}
              style={{ marginTop: 24 }}
            >
              Áp dụng
            </Button>
          </Col>
        </Row>
      </Card>

      <Spin spinning={loading}>
        <Row gutter={[24, 24]} style={{ marginBottom: 32 }}>
          <Col xs={24} sm={12} lg={6}>
            <StatsCard
              title="Tổng số công việc"
              value={reportData?.totalTasks || 0}
              prefix={<TrophyOutlined />}
              className="stat-card stat-card-primary"
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <StatsCard
              title="Đã hoàn thành"
              value={reportData?.completedTasks || 0}
              suffix={`/ ${reportData?.totalTasks || 0}`}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#ffffffbe', fontSize: '32px', fontWeight: 'bold' }}
              className="stat-card stat-card-success"
              showProgress
              progressPercent={completionRate}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <StatsCard
              title="Đang thực hiện"
              value={reportData?.inProgressTasks || 0}
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: '#ffffffbe', fontSize: '32px', fontWeight: 'bold' }}
              className="stat-card stat-card-warning"
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <StatsCard
              title="Quá hạn"
              value={reportData?.overdueTasks || 0}
              prefix={<WarningOutlined />}
              valueStyle={{ color: '#ffffffbe', fontSize: '32px', fontWeight: 'bold' }}
              className="stat-card stat-card-danger"
            />
          </Col>
        </Row>

        <Card 
          title={
            <Space>
              <LineChartOutlined style={{ color: '#1890ff' }} />
              <span>Biểu đồ hoàn thành</span>
              <Tag color="blue">
                {dateRange[0].format('DD/MM')} - {dateRange[1].format('DD/MM/YYYY')}
              </Tag>
            </Space>
          }
          style={{ marginBottom: 24 }}
        >
          {!reportData?.completionTrend || reportData.completionTrend.length === 0 ? (
            <Empty 
              description="Chưa có dữ liệu xu hướng"
              image={Empty.PRESENTED_IMAGE_SIMPLE}
            />
          ) : (
            <Line
              data={reportData.completionTrend.map((item: { date: string; completedCount: number }) => ({
                date: dayjs(item.date).format('DD/MM'),
                value: item.completedCount,
                type: 'Hoàn thành'
              }))}
              xField="date"
              yField="value"
              seriesField="type"
              smooth={true}
              yAxis={{
                label: {
                  formatter: (v: string) => `${Math.round(Number(v))}`,
                },
                tickCount: 5,
                nice: true,
              }}
              animation={{
                appear: {
                  animation: 'path-in',
                  duration: 1000,
                },
              }}
              tooltip={{
                formatter: (datum: { value: number }) => {
                  return {
                    name: 'Số lượng',
                    value: `${datum.value} tasks`,
                  };
                },
              }}
              point={{
                size: 5,
                shape: 'circle',
                style: {
                  fill: 'white',
                  stroke: '#1890ff',
                  lineWidth: 2,
                },
              }}
              color="#1890ff"
              height={300}
            />
          )}
        </Card>

        <Row gutter={[24, 24]} style={{ marginBottom: 32 }}>
          <Col xs={24} lg={12}>
            <Card 
              title={
                <Space>
                  <RocketOutlined style={{ color: '#1890ff' }} />
                  <span>Năng suất của bạn</span>
                </Space>
              }
            >
              <Row gutter={16}>
                <Col span={8}>
                  <div className="productivity-item">
                    <Text type="secondary">Hôm nay</Text>
                    <div className="productivity-value">
                      {reportData?.tasksCompletedThisToday || 0}
                      <Text type="secondary" style={{ fontSize: '14px', marginLeft: 4 }}>tasks</Text>
                    </div>
                  </div>
                </Col>
                <Col span={8}>
                  <div className="productivity-item">
                    <Text type="secondary">Tuần này</Text>
                    <div className="productivity-value">
                      {reportData?.tasksCompletedThisWeek || 0}
                      <Text type="secondary" style={{ fontSize: '14px', marginLeft: 4 }}>tasks</Text>
                    </div>
                  </div>
                </Col>
                <Col span={8}>
                  <div className="productivity-item">
                    <Text type="secondary">Tháng này</Text>
                    <div className="productivity-value">
                      {reportData?.tasksCompletedThisMonth || 0}
                      <Text type="secondary" style={{ fontSize: '14px', marginLeft: 4 }}>tasks</Text>
                    </div>
                  </div>
                </Col>
              </Row>
              <div style={{ marginTop: 24, padding: '16px', background: '#f0f2f5', borderRadius: 8 }}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Text strong>Thời gian hoàn thành TB:</Text>
                    <Text>{reportData?.averageCompletionTimeHours.toFixed(1) || 0} giờ</Text>
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Text strong>Tỷ lệ hoàn thành:</Text>
                    <Text style={{ color: completionRate >= 70 ? '#52c41a' : '#faad14' }}>
                      {completionRate}%
                    </Text>
                  </div>
                </Space>
              </div>
            </Card>
          </Col>

          <Col xs={24} lg={12}>
            <Card 
              title={
                <Space>
                  <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
                  <span>Cần chú ý</span>
                </Space>
              }
            >
              <Row gutter={16}>
                <Col span={8}>
                  <div className="attention-item attention-high">
                    <Text type="secondary">Ưu tiên cao</Text>
                    <div className="attention-value">
                      {reportData?.highPriorityPendingTasks || 0}
                    </div>
                  </div>
                </Col>
                <Col span={8}>
                  <div className="attention-item attention-medium">
                    <Text type="secondary">Ưu tiên TB</Text>
                    <div className="attention-value">
                      {reportData?.mediumPriorityPendingTasks || 0}
                    </div>
                  </div>
                </Col>
                <Col span={8}>
                  <div className="attention-item attention-low">
                    <Text type="secondary">Ưu tiên thấp</Text>
                    <div className="attention-value">
                      {reportData?.lowPriorityPendingTasks || 0}
                    </div>
                  </div>
                </Col>
              </Row>
              {(reportData?.overdueTasks || 0) > 0 && (
                <div style={{ marginTop: 24, padding: '12px', background: '#fff2e8', borderRadius: 8, border: '1px solid #ffbb96' }}>
                  <Space>
                    <WarningOutlined style={{ color: '#ff4d4f' }} />
                    <Text strong style={{ color: '#ff4d4f' }}>
                      Bạn có {reportData?.overdueTasks} công việc quá hạn!
                    </Text>
                  </Space>
                </div>
              )}
            </Card>
          </Col>
        </Row>

        <Row gutter={[24, 24]}>
          <Col xs={24}>
            <Card
              title={
                <Space>
                  <WarningOutlined style={{ color: '#ff4d4f' }} />
                  <span>Quá hạn</span>
                </Space>
              }
              extra={
                <Button type="link" onClick={() => navigate('/')}>
                  Xem tất cả →
                </Button>
              }
            >
              {!reportData?.mostOverdueTasks || reportData.mostOverdueTasks.length === 0 ? (
                <Empty 
                  description="Tuyệt vời! Không có công việc quá hạn"
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                />
              ) : (
                <List
                  dataSource={reportData.mostOverdueTasks.slice(0, 5)}
                  renderItem={(item: { title: string; priority: number; dueDate: string | dayjs.Dayjs }) => {
                    const dueDateDayjs = typeof item.dueDate === 'string' ? dayjs(item.dueDate) : item.dueDate;
                    const daysOverdue = dayjs().diff(dueDateDayjs, 'day');
                    
                    return (
                      <List.Item
                        extra={
                          <Tag color="red">
                            {daysOverdue} ngày
                          </Tag>
                        }
                      >
                        <List.Item.Meta
                          avatar={<WarningOutlined style={{ color: 'red', fontSize: 18 }} />}
                          title={<Text strong>{item.title}</Text>}
                          description={
                            <Space>
                              <PriorityTag priority={item.priority} />
                              <Text type="secondary" delete>Hạn: {dueDateDayjs.format('DD/MM/YYYY')}</Text>
                            </Space>
                          }
                        />
                      </List.Item>
                    );
                  }}
                />
              )}
            </Card>
          </Col>
        </Row>

        <Card 
          title="Hành động nhanh" 
          style={{ marginTop: 24 }}
        >
          <Space size="large" wrap>
            <Button 
              type="primary" 
              size="large"
              onClick={() => navigate('/')}
            >
              Quản lý công việc
            </Button>
            <Button 
              size="large"
              onClick={fetchDashboardData}
              loading={loading}
            >
              Làm mới dữ liệu
            </Button>
          </Space>
        </Card>
      </Spin>
    </div>
  );
};

export default Dashboard;
