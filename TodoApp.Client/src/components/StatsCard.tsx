import { Card, Statistic, Progress } from 'antd';
import type { ReactNode } from 'react';

interface StatsCardProps {
  title: string;
  value: number;
  prefix?: ReactNode;
  suffix?: string | ReactNode;
  valueStyle?: React.CSSProperties;
  className?: string;
  showProgress?: boolean;
  progressPercent?: number;
  progressColor?: string;
  total?: number;
}

/**
 * Component Card thống kê với style nhất quán
 * @param title - Tiêu đề thống kê
 * @param value - Giá trị hiển thị
 * @param prefix - Icon hoặc element phía trước
 * @param suffix - Đơn vị hoặc text phía sau (vd: "/ 10", "tasks")
 * @param valueStyle - Custom style cho số liệu
 * @param className - Custom CSS class (vd: "stat-card-success")
 * @param showProgress - Có hiển thị thanh Progress không
 * @param progressPercent - Phần trăm Progress (nếu có)
 * @param progressColor - Màu thanh Progress
 * @param total - Tổng số (để tự động tính percent)
 * 
 * @example
 * <StatsCard 
 *   title="Tổng công việc"
 *   value={50}
 *   prefix={<TrophyOutlined />}
 *   valueStyle={{ color: '#1890ff' }}
 * />
 * 
 * <StatsCard 
 *   title="Đã hoàn thành"
 *   value={30}
 *   total={50}
 *   showProgress
 *   className="stat-card-success"
 * />
 */
export const StatsCard = ({
  title,
  value,
  prefix,
  suffix,
  valueStyle,
  className,
  showProgress = false,
  progressPercent,
  progressColor = '#52c41a',
  total,
}: StatsCardProps) => {
  // Tự động tính percent nếu có total
  const calculatedPercent = total ? Math.round((value / Math.max(total, 1)) * 100) : progressPercent;
  
  return (
    <Card className={className}>
      <Statistic
        title={title}
        value={value}
        prefix={prefix}
        suffix={suffix}
        valueStyle={valueStyle || { color: '#1890ff', fontSize: '32px', fontWeight: 'bold' }}
      />
      {showProgress && calculatedPercent !== undefined && (
        <Progress
          percent={calculatedPercent}
          strokeColor={progressColor}
          size="small"
          style={{ marginTop: 8 }}
        />
      )}
    </Card>
  );
};
