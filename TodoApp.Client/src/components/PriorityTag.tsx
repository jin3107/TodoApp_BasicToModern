import { Tag } from 'antd';
import { Tier } from '../commons';

interface PriorityTagProps {
  priority: Tier | number;
}

/**
 * Component hiển thị tag độ ưu tiên
 * @param priority - Độ ưu tiên (Tier enum hoặc number)
 * @returns Tag với màu sắc tương ứng
 */
export const PriorityTag = ({ priority }: PriorityTagProps) => {
  const priorityValue = typeof priority === 'number' ? priority : priority;
  
  switch (priorityValue) {
    case Tier.High:
    case 2:
      return <Tag color="red">Cao</Tag>;
    case Tier.Medium:
    case 1:
      return <Tag color="orange">Trung bình</Tag>;
    case Tier.Low:
    case 0:
      return <Tag color="green">Thấp</Tag>;
    default:
      return <Tag>Không xác định</Tag>;
  }
};
