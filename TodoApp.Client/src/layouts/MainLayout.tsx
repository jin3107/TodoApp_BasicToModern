import { useState } from 'react';
import { Link, useLocation, Outlet, useNavigate } from 'react-router-dom';
import { App, Avatar, Button, Drawer, Dropdown, Grid, Layout, Menu, Space, Typography } from 'antd';
import type { MenuProps } from 'antd';
import {
  DownOutlined,
  KeyOutlined,
  LogoutOutlined,
  MenuOutlined,
  MoonOutlined,
  SunOutlined,
  UserOutlined,
} from '@ant-design/icons';
import { logout } from '../apis/authenticationAPI';
import { clearAuthenticated } from '../commons/auth-session';
import { useAppTheme } from '../commons/ThemeContext';
import { ChecklistLogo } from '../components';
import './MainLayout.scss';

const { Header, Content } = Layout;
const { Text } = Typography;
const { useBreakpoint } = Grid;

const NAV_ITEMS = [
  { key: '/dashboard', label: 'Trang chủ' },
  { key: '/todo-lists', label: 'Quản lý công việc' },
  { key: '/reports', label: 'Báo cáo' },
];

const MainLayout = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { message } = App.useApp();
  const { theme, toggleTheme } = useAppTheme();
  const screens = useBreakpoint();
  const [drawerOpen, setDrawerOpen] = useState(false);
  const isDesktop = Boolean(screens.md);

  const handleLogout = async () => {
    try {
      await logout();
      message.success('Đã đăng xuất');
    } catch {
      message.warning('Phiên đăng nhập đã kết thúc');
    } finally {
      clearAuthenticated();
      navigate('/login', { replace: true });
    }
  };

  const accountMenuItems: MenuProps['items'] = [
    {
      key: 'change-password',
      label: 'Đổi mật khẩu',
      icon: <KeyOutlined />,
      onClick: () => navigate('/change-password'),
    },
    {
      type: 'divider',
    },
    {
      key: 'logout',
      label: 'Đăng xuất',
      icon: <LogoutOutlined />,
      danger: true,
      onClick: handleLogout,
    },
  ];

  const closeDrawer = () => setDrawerOpen(false);

  const drawerMenuItems = NAV_ITEMS.map((item) => ({
    key: item.key,
    label: <Link to={item.key}>{item.label}</Link>,
  }));

  return (
    <Layout className="main-layout">
      <Header className="header">
        <div className="header-left">
          {!isDesktop && (
            <Button
              type="text"
              icon={<MenuOutlined />}
              className="menu-button"
              aria-label="Mở menu"
              onClick={() => setDrawerOpen(true)}
            />
          )}
          <div className="brand" onClick={() => navigate('/dashboard')}>
            <ChecklistLogo size={20} />
            <Text strong className="brand-text">
              TodoApp
            </Text>
          </div>
          {isDesktop && (
            <nav className="main-nav">
              {NAV_ITEMS.map((item) => (
                <Link
                  key={item.key}
                  to={item.key}
                  className={`main-nav-item ${location.pathname === item.key ? 'active' : ''}`}
                >
                  {item.label}
                </Link>
              ))}
            </nav>
          )}
        </div>
        <div className="header-right">
          <Button
            type="text"
            className="theme-toggle"
            aria-label="Chuyển giao diện sáng/tối"
            icon={theme === 'dark' ? <SunOutlined /> : <MoonOutlined />}
            onClick={toggleTheme}
          />
          {isDesktop && (
            <Dropdown
              menu={{ items: accountMenuItems }}
              trigger={['hover']}
              placement="bottomRight"
              arrow
            >
              <button className="user-menu-button" type="button" aria-label="Tài khoản">
                <Avatar icon={<UserOutlined />} className="user-avatar" />
                <Text className="user-name">Admin</Text>
                <DownOutlined className="user-menu-caret" />
              </button>
            </Dropdown>
          )}
        </div>
      </Header>
      <Drawer
        title={
          <Space className="drawer-brand">
            <ChecklistLogo size={18} />
            <Text strong>TodoApp</Text>
          </Space>
        }
        placement="left"
        open={drawerOpen}
        onClose={closeDrawer}
        className="navigation-drawer"
        width={300}
      >
        <Menu
          mode="inline"
          selectedKeys={[location.pathname]}
          items={drawerMenuItems}
          onClick={closeDrawer}
        />
        <Button
          type="text"
          icon={<KeyOutlined />}
          block
          className="drawer-change-password"
          onClick={() => {
            closeDrawer();
            navigate('/change-password');
          }}
        >
          Đổi mật khẩu
        </Button>
        <Button
          icon={<LogoutOutlined />}
          danger
          block
          className="drawer-logout"
          onClick={handleLogout}
        >
          Đăng xuất
        </Button>
      </Drawer>
      <Layout>
        <Content className="content">
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
};

export default MainLayout;
