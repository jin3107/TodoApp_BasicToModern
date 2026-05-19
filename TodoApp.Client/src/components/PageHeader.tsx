import { Typography } from 'antd';
import type { ReactNode } from 'react';

const { Title, Paragraph } = Typography;

interface PageHeaderProps {
  title: string;
  subtitle?: string;
  greeting?: boolean;
  actions?: ReactNode;
  className?: string;
}

/**
 * Component Header nhất quán cho các pages
 * @param title - Tiêu đề chính
 * @param subtitle - Mô tả phụ (optional)
 * @param greeting - Tự động thêm lời chào theo giờ (optional)
 * @param actions - Các button hoặc action (optional)
 * @param className - Custom CSS class
 * 
 * @example
 * <PageHeader 
 *   title="Quản lý công việc"
 *   subtitle="Xem và quản lý tất cả công việc của bạn"
 *   actions={<Button type="primary">Thêm mới</Button>}
 * />
 * 
 * <PageHeader 
 *   title="Dashboard"
 *   greeting
 *   subtitle="Đây là tổng quan về công việc của bạn"
 * />
 */
export const PageHeader = ({ 
  title, 
  subtitle, 
  greeting = false, 
  actions,
  className = 'page-header'
}: PageHeaderProps) => {
  const getGreeting = () => {
    const hour = new Date().getHours();
    if (hour < 12) return 'Chào buổi sáng';
    if (hour < 18) return 'Chào buổi chiều';
    return 'Chào buổi tối';
  };

  const displayTitle = greeting ? `${getGreeting()}!` : title;

  return (
    <div className={className}>
      <div>
        <Title level={2} className="page-header__title">
          {displayTitle}
        </Title>
        {subtitle && (
          <Paragraph className="page-header__subtitle">
            {subtitle}
          </Paragraph>
        )}
      </div>
      {actions && <div className="page-header__actions">{actions}</div>}
    </div>
  );
};
