import { Tag } from 'antd';
import { CheckCircleOutlined, ClockCircleOutlined, WarningOutlined } from '@ant-design/icons';

interface StatusTagProps {
  isCompleted: boolean;
  isOverdue?: boolean;
}

export const StatusTag = ({ isCompleted, isOverdue }: StatusTagProps) => {
  if (isCompleted) {
    return (
      <Tag icon={<CheckCircleOutlined />} color="#a06f24">
        Hoàn thành
      </Tag>
    );
  }

  if (isOverdue) {
    return (
      <Tag icon={<WarningOutlined />} color="#5a3b0a">
        Quá hạn
      </Tag>
    );
  }

  return (
    <Tag icon={<ClockCircleOutlined />} color="default">
      Chưa hoàn thành
    </Tag>
  );
};
