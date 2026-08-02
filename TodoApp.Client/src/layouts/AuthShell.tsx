import type { ReactNode } from 'react';
import { Button } from 'antd';
import { MoonOutlined, SunOutlined } from '@ant-design/icons';
import { useAppTheme } from '../commons/ThemeContext';
import { ChecklistLogo } from '../components';
import authIllustration from '../assets/auth-illustration.png';
import './AuthShell.scss';

interface AuthShellProps {
  children: ReactNode;
}

const AuthShell = ({ children }: AuthShellProps) => {
  const { theme, toggleTheme } = useAppTheme();

  return (
    <main className="auth-shell">
      <Button
        type="text"
        className="auth-shell__theme-toggle"
        aria-label="Chuyển giao diện sáng/tối"
        icon={theme === 'dark' ? <SunOutlined /> : <MoonOutlined />}
        onClick={toggleTheme}
      />
      <div className="auth-shell__brand">
        <ChecklistLogo size={24} />
        <span>TodoApp</span>
      </div>
      <div className="auth-shell__row">
        <img className="auth-shell__image" src={authIllustration} alt="Minh họa quản lý công việc" />
        <div className="auth-shell__panel">{children}</div>
      </div>
    </main>
  );
};

export default AuthShell;
