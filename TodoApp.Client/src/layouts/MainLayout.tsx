import { Link, useLocation, Outlet, useNavigate } from 'react-router-dom';
import { App, Avatar, Button, Layout, Menu, Space, Typography } from 'antd';
import {
  CheckSquareOutlined,
  KeyOutlined,
  LogoutOutlined,
  UserOutlined,
} from '@ant-design/icons';
import { logout } from '../apis/authenticationAPI';
import './MainLayout.scss';

const { Header, Content } = Layout;
const { Text } = Typography;

const MainLayout = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { message } = App.useApp();

  const menuItems = [
    {
      key: '/dashboard',
      label: <Link to="/dashboard">Trang chủ</Link>,
    },
    {
      key: '/todo-lists',
      label: <Link to="/todo-lists">Quản lý công việc</Link>,
    },
    {
      key: '/change-password',
      label: <Link to="/change-password">Đổi mật khẩu</Link>,
      icon: <KeyOutlined />,
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

  return (
    <Layout className="main-layout">
      <Header className="header">
        <div className="header-left">
          <Space className="brand">
            <CheckSquareOutlined className="brand-icon" />
            <Text strong className="brand-text">
              Todo App
            </Text>
          </Space>
          <Menu
            theme="dark"
            mode="horizontal"
            selectedKeys={[location.pathname]}
            items={menuItems}
            className="main-menu"
            overflowedIndicator={null}
          />
        </div>
        <Space className="user-info">
          <Avatar icon={<UserOutlined />} className="user-avatar" />
          <Text className="user-name">Admin</Text>
          <Button
            type="text"
            icon={<LogoutOutlined />}
            className="logout-button"
            onClick={handleLogout}
          >
            Đăng xuất
          </Button>
        </Space>
      </Header>
      <Layout>
        <Content className="content">
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
};

export default MainLayout;
