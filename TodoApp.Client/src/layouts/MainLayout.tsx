import { useState } from 'react';
import { Link, useLocation, Outlet, useNavigate } from 'react-router-dom';
import { App, Avatar, Button, Drawer, Dropdown, Grid, Layout, Menu, Space, Typography } from 'antd';
import type { MenuProps } from 'antd';
import {
  CheckSquareOutlined,
  DownOutlined,
  HomeOutlined,
  KeyOutlined,
  LogoutOutlined,
  MenuOutlined,
  UnorderedListOutlined,
  UserOutlined,
} from '@ant-design/icons';
import { logout } from '../apis/authenticationAPI';
import './MainLayout.scss';

const { Header, Content } = Layout;
const { Text } = Typography;
const { useBreakpoint } = Grid;

const MainLayout = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { message } = App.useApp();
  const screens = useBreakpoint();
  const [drawerOpen, setDrawerOpen] = useState(false);
  const isDesktop = Boolean(screens.md);

  const menuItems = [
    {
      key: '/dashboard',
      label: <Link to="/dashboard">Trang chủ</Link>,
      icon: <HomeOutlined />,
    },
    {
      key: '/todo-lists',
      label: <Link to="/todo-lists">Quản lý công việc</Link>,
      icon: <UnorderedListOutlined />,
    },
  ];

  const handleLogout = async () => {
    try {
      await logout();
      message.success('Đã đăng xuất');
    } catch {
      message.warning('Phiên đăng nhập đã kết thúc');
    } finally {
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
          <Space className="brand">
            <CheckSquareOutlined className="brand-icon" />
            <Text strong className="brand-text">
              Todo App
            </Text>
          </Space>
          {isDesktop && (
            <Menu
              theme="dark"
              mode="horizontal"
              selectedKeys={[location.pathname]}
              items={menuItems}
              className="main-menu"
            />
          )}
        </div>
        <Dropdown
          menu={{ items: accountMenuItems }}
          trigger={isDesktop ? ['hover'] : ['click']}
          placement="bottomRight"
          arrow
        >
          <button className="user-menu-button" type="button" aria-label="Tài khoản">
            <Avatar icon={<UserOutlined />} className="user-avatar" />
            <Text className="user-name">Admin</Text>
            <DownOutlined className="user-menu-caret" />
          </button>
        </Dropdown>
      </Header>
      <Drawer
        title={
          <Space className="drawer-brand">
            <CheckSquareOutlined />
            <Text strong>Todo App</Text>
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
          items={menuItems}
          onClick={closeDrawer}
        />
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
