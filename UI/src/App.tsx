import { BrowserRouter as Router, Routes, Route, Link, useLocation } from 'react-router-dom';
import { Layout, Menu, theme, Typography, Space, Avatar } from 'antd';
import { 
  DashboardOutlined,
  CheckSquareOutlined, 
  UserOutlined,
} from '@ant-design/icons';
import Dashboard from './pages/Dashboard';
import Tasks from './pages/Tasks';
import './App.css';

const { Header, Content, Sider } = Layout;
const { Text } = Typography;

/**
 * App Component - Main application layout
 * 
 * Features:
 * - Responsive sidebar navigation
 * - Modern UI with Ant Design
 * - Clean routing structure
 * - Professional header with branding
 */
function AppContent() {
  const location = useLocation();
  const {
    token: { colorBgContainer },
  } = theme.useToken();

  const menuItems = [
    {
      key: '/dashboard',
      icon: <DashboardOutlined />,
      label: <Link to="/dashboard">Trang chủ</Link>,
    },
    {
      key: '/',
      icon: <CheckSquareOutlined />,
      label: <Link to="/">Quản lý công việc</Link>,
    },
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        breakpoint="lg"
        collapsedWidth="0"
        style={{
          overflow: 'auto',
          height: '100vh',
          position: 'fixed',
          left: 0,
          top: 0,
          bottom: 0,
        }}
      >
        <div style={{ 
          height: 64, 
          display: 'flex', 
          alignItems: 'center', 
          justifyContent: 'center',
          padding: '0 24px',
          background: 'rgba(255, 255, 255, 0.1)',
        }}>
          <Space>
            <CheckSquareOutlined style={{ fontSize: 24, color: '#1890ff' }} />
            <Text strong style={{ color: 'white', fontSize: 18 }}>
              Todo App
            </Text>
          </Space>
        </div>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[location.pathname === '/dashboard' || location.pathname === '/' ? location.pathname : location.pathname]}
          items={menuItems}
          style={{ marginTop: 16 }}
        />
      </Sider>
      <Layout style={{ marginLeft: 200 }}>
        <Header 
          style={{ 
            padding: '0 24px', 
            background: colorBgContainer,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
          }}
        >
          <Text strong style={{ fontSize: 16 }}>
            {location.pathname === '/dashboard' ? 'Dashboard' :
             location.pathname === '/' ? 'Quản lý công việc' : 'Todo App'}
          </Text>
          <Space>
            <Avatar icon={<UserOutlined />} style={{ backgroundColor: '#1890ff' }} />
            <Text>Admin</Text>
          </Space>
        </Header>
        <Content style={{ margin: '24px', background: '#f0f2f5', minHeight: 'calc(100vh - 112px)' }}>
          <Routes>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/" element={<Tasks />} />
          </Routes>
        </Content>
      </Layout>
    </Layout>
  );
}

function App() {
  return (
    <Router>
      <AppContent />
    </Router>
  );
}

export default App;
