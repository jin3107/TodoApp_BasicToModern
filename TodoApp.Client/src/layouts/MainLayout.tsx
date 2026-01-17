import { Link, useLocation, Outlet } from 'react-router-dom';
import { Layout, Menu, Typography, Space, Avatar } from 'antd';
import {
  CheckSquareOutlined,
  UserOutlined,
} from '@ant-design/icons';
import './MainLayout.scss';

const { Header, Content } = Layout;
const { Text } = Typography;

const MainLayout = () => {
  const location = useLocation();

  const menuItems = [
    {
      key: '/dashboard',
      label: <Link to="/dashboard">Trang chủ</Link>,
    },
    {
      key: '/',
      label: <Link to="/">Quản lý công việc</Link>,
    },
  ];

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
