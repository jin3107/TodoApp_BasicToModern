import { Tag } from 'antd';
import { Tier } from '../commons/enums/Tier';

interface PriorityTagProps {
  priority: Tier | number;
}

export const PriorityTag = ({ priority }: PriorityTagProps) => {
  const priorityValue = typeof priority === 'number' ? priority : priority;
  
  switch (priorityValue) {
    case Tier.High:
    case 2:
      return <Tag color="#7d5411">Cao</Tag>;
    case Tier.Medium:
    case 1:
      return <Tag color="#c28d41">Trung bình</Tag>;
    case Tier.Low:
    case 0:
      return <Tag color="default">Thấp</Tag>;
    default:
      return <Tag>Không xác định</Tag>;
  }
};
