import { useState } from "react";
import { useNavigate } from "react-router-dom";
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
  CheckCircleOutlined,
  KeyOutlined,
  LockOutlined,
  MailOutlined,
  SendOutlined,
} from "@ant-design/icons";
import { OtpPurpose } from "../../commons/enums/OtpPupose";
import type { ChangePasswordRequest, VerifyOtpRequest } from "../../interfaces";
import {
  changePassword,
  logout,
  sendOtp,
  verifyOtp,
} from "../../apis/authenticationAPI";
import "./style.scss";

type ChangePasswordForm = ChangePasswordRequest & Pick<VerifyOtpRequest, "code">;

const { Text, Title } = Typography;

const ChangePassword = () => {
  const navigate = useNavigate();
  const { message } = App.useApp();
  const [form] = Form.useForm<ChangePasswordForm>();
  const [otpSent, setOtpSent] = useState(false);
  const [otpVerified, setOtpVerified] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSendOtp = async () => {
    const values = await form.validateFields(["email"]);
    setLoading(true);
    try {
      const result = await sendOtp({
        email: values.email,
        purpose: OtpPurpose.ChangePassword,
      });

      if (!result.isSuccess) {
        message.error(result.message || "Không thể gửi OTP");
        return;
      }

      setOtpSent(true);
      setOtpVerified(false);
      message.success(result.message || "Đã gửi OTP");
    } finally {
      setLoading(false);
    }
  };

  const handleVerifyOtp = async () => {
    const values = await form.validateFields(["email", "code"]);
    setLoading(true);
    try {
      const result = await verifyOtp({
        email: values.email,
        code: values.code,
        purpose: OtpPurpose.ChangePassword,
      });

      if (!result.isSuccess) {
        message.error(result.message || "Xác minh OTP thất bại");
        return;
      }

      setOtpVerified(true);
      message.success("OTP đã được xác minh");
    } finally {
      setLoading(false);
    }
  };

  const handleChangePassword = async (values: ChangePasswordForm) => {
    if (!otpVerified) {
      message.warning("Vui lòng xác minh OTP trước");
      return;
    }

    setLoading(true);
    try {
      const result = await changePassword({
        email: values.email,
        newPassword: values.newPassword,
        confirmNewPassword: values.confirmNewPassword,
      });

      if (!result.isSuccess) {
        message.error(result.message || "Đổi mật khẩu thất bại");
        return;
      }

      await logout().catch(() => undefined);
      message.success("Mật khẩu đã được thay đổi. Vui lòng đăng nhập lại");
      navigate("/login", { replace: true });
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="change-password-page">
      <Card className="change-password-page__card" variant="borderless">
        <Space direction="vertical" className="change-password-page__header">
          <KeyOutlined className="change-password-page__icon" />
          <Title level={2}>Đổi mật khẩu</Title>
          <Text type="secondary">
            Xác minh email bằng OTP trước khi cập nhật mật khẩu mới.
          </Text>
        </Space>

        <Form
          form={form}
          layout="vertical"
          requiredMark={false}
          onFinish={handleChangePassword}
        >
          <Form.Item
            label="Email"
            name="email"
            rules={[
              { required: true, message: "Vui lòng nhập email" },
              { type: "email", message: "Email không hợp lệ" },
            ]}
          >
            <Input
              size="large"
              prefix={<MailOutlined />}
              disabled={otpVerified}
            />
          </Form.Item>

          <Button
            type="primary"
            icon={<SendOutlined />}
            onClick={handleSendOtp}
            loading={loading}
            disabled={otpVerified}
            block
          >
            {otpSent ? "Gửi lại OTP" : "Gửi OTP"}
          </Button>

          <Form.Item
            label="Mã OTP"
            name="code"
            className="change-password-page__otp"
            rules={[
              { required: true, message: "Vui long nhap OTP" },
              { len: 6, message: "OTP gồm 6 ký tự" },
            ]}
          >
            <Input
              size="large"
              maxLength={6}
              inputMode="numeric"
              disabled={!otpSent || otpVerified}
            />
          </Form.Item>

          <Button
            type="primary"
            icon={<CheckCircleOutlined />}
            onClick={handleVerifyOtp}
            loading={loading}
            disabled={!otpSent || otpVerified}
            block
          >
            {otpVerified ? "Đã xác minh OTP" : "Xác minh OTP"}
          </Button>

          <Form.Item
            label="Mật khẩu mới"
            name="newPassword"
            className="change-password-page__password"
            rules={[
              { required: true, message: "Vui lòng nhập mật khẩu mới" },
              { min: 8, message: "Mật khẩu tối thiểu 8 ký tự" },
              { max: 40, message: "Mật khẩu tối đa 40 ký tự" },
            ]}
          >
            <Input.Password
              size="large"
              prefix={<LockOutlined />}
              disabled={!otpVerified}
              autoComplete="new-password"
            />
          </Form.Item>

          <Form.Item
            label="Xác nhận mật khẩu mới"
            name="confirmNewPassword"
            dependencies={["newPassword"]}
            rules={[
              { required: true, message: "Vui lòng xác nhận mật khẩu mới" },
              ({ getFieldValue }) => ({
                validator(_, value: string | undefined) {
                  if (!value || getFieldValue("newPassword") === value) {
                    return Promise.resolve();
                  }
                  return Promise.reject(new Error("Mật khẩu xác nhận không khớp"));
                },
              }),
            ]}
          >
            <Input.Password
              size="large"
              prefix={<LockOutlined />}
              disabled={!otpVerified}
              autoComplete="new-password"
            />
          </Form.Item>

          <Button
            type="primary"
            htmlType="submit"
            size="large"
            icon={<KeyOutlined />}
            loading={loading}
            disabled={!otpVerified}
            block
          >
            Cập nhật mật khẩu
          </Button>
        </Form>
      </Card>
    </main>
  );
};

export default ChangePassword;
