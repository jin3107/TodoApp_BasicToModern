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

export const StatsCard = ({
  title,
  value,
  prefix,
  suffix,
  valueStyle,
  className,
  showProgress = false,
  progressPercent,
  progressColor = '#b68235',
  total,
}: StatsCardProps) => {
  const calculatedPercent = total ? Math.round((value / Math.max(total, 1)) * 100) : progressPercent;
  
  return (
    <Card className={className}>
      <Statistic
        title={title}
        value={value}
        prefix={prefix}
        suffix={suffix}
        valueStyle={valueStyle || { color: '#b68235', fontSize: '32px', fontWeight: 'bold' }}
      />
      {showProgress && calculatedPercent !== undefined && (
        <Progress
          percent={calculatedPercent}
          strokeColor={progressColor}
          size="small"
          className="stats-card-progress"
        />
      )}
    </Card>
  );
};
