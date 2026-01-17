import { Tag } from 'antd';
import { CheckCircleOutlined, ClockCircleOutlined } from '@ant-design/icons';

interface StatusTagProps {
  isCompleted: boolean;
}

/**
 * Component hiển thị tag trạng thái hoàn thành
 * @param isCompleted - Trạng thái đã hoàn thành hay chưa
 * @returns Tag với icon và màu sắc tương ứng
 */
export const StatusTag = ({ isCompleted }: StatusTagProps) => {
  return isCompleted ? (
    <Tag icon={<CheckCircleOutlined />} color="success">
      Hoàn thành
    </Tag>
  ) : (
    <Tag icon={<ClockCircleOutlined />} color="processing">
      Chưa hoàn thành
    </Tag>
  );
};
