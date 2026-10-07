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
