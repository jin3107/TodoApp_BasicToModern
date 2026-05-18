import { Link, useNavigate } from "react-router-dom";
import {
  App,
  Button,
  Card,
  Form,
  Input,
  Space,
  Typography,
} from "antd";
import {
  LockOutlined,
  LoginOutlined,
  UserOutlined,
} from "@ant-design/icons";
import type { LoginRequest } from "../../interfaces";
import { login } from "../../apis/authenticationAPI";
import "./style.scss";

const { Text, Title } = Typography;

const Login = () => {
  const navigate = useNavigate();
  const { message } = App.useApp();
  const [form] = Form.useForm<LoginRequest>();

  const handleSubmit = async (values: LoginRequest) => {
    const result = await login(values);

    if (!result.isSuccess) {
      message.error(result.message || "Đăng nhập thất bại");
      return;
    }

    message.success("Đăng nhập thành công");
    form.resetFields();
    navigate("/dashboard", { replace: true });
  };

  return (
    <main className="login-page">
      <section className="login-page__panel">
        <div className="login-page__intro">
          <Text className="login-page__eyebrow">Todo App</Text>
          <Title level={1}>Đăng nhập</Title>
          <Text className="login-page__subtitle">
            Quản lý danh sách công việc, tiến độ và báo cáo trong một nơi.
          </Text>
        </div>

        <Card className="login-page__card" variant="borderless">
          <Form
            form={form}
            layout="vertical"
            requiredMark={false}
            onFinish={handleSubmit}
          >
            <Form.Item
              label="Email hoặc tên đăng nhập"
              name="userName"
              rules={[
                { required: true, message: "Vui lòng nhập email hoặc tên đăng nhập" },
              ]}
            >
              <Input
                size="large"
                prefix={<UserOutlined />}
                placeholder="you@example.com"
                autoComplete="username"
              />
            </Form.Item>

            <Form.Item
              label="Mật khẩu"
              name="password"
              rules={[{ required: true, message: "Vui lòng nhập mật khẩu" }]}
            >
              <Input.Password
                size="large"
                prefix={<LockOutlined />}
                placeholder="Nhập mật khẩu"
                autoComplete="current-password"
              />
            </Form.Item>

            <Button
              type="primary"
              htmlType="submit"
              size="large"
              icon={<LoginOutlined />}
              block
            >
              Đăng nhập
            </Button>
          </Form>

          <Space className="login-page__footer">
            <Text type="secondary">Chưa có tài khoản?</Text>
            <Link to="/register">Đăng ký ngay</Link>
          </Space>
        </Card>
      </section>
    </main>
  );
};

export default Login;
