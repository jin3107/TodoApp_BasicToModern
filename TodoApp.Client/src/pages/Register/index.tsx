import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  App,
  Button,
  Card,
  Form,
  Input,
  Space,
  Steps,
  Typography,
} from "antd";
import {
  CheckCircleOutlined,
  LockOutlined,
  MailOutlined,
  PhoneOutlined,
  UserAddOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { OtpPurpose } from "../../commons/enums/OtpPupose";
import type { RegisterRequest, VerifyOtpRequest } from "../../interfaces";
import { register, sendOtp, verifyOtp } from "../../apis/authenticationAPI";
import authIllustration from "../../assets/auth-illustration.png";
import { scheduleAuthenticatedRoutesPreload } from "../../routes/preload";
import "./style.scss";

const { Text, Title } = Typography;

const Register = () => {
  const navigate = useNavigate();
  const { message } = App.useApp();
  const [registerForm] = Form.useForm<RegisterRequest>();
  const [otpForm] = Form.useForm<Pick<VerifyOtpRequest, "code">>();
  const [step, setStep] = useState(0);
  const [registeredEmail, setRegisteredEmail] = useState("");
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    scheduleAuthenticatedRoutesPreload();
  }, []);

  const handleRegister = async (values: RegisterRequest) => {
    setSubmitting(true);
    try {
      const result = await register(values);
      if (!result.isSuccess) {
        message.error(result.message || "đăng ký thất bại");
        return;
      }

      setRegisteredEmail(values.email);
      setStep(1);
      message.success(result.message || "Vui lòng kiểm tra email để lấy OTP");
    } finally {
      setSubmitting(false);
    }
  };

  const handleVerify = async (values: Pick<VerifyOtpRequest, "code">) => {
    setSubmitting(true);
    try {
      const result = await verifyOtp({
        email: registeredEmail,
        code: values.code,
        purpose: OtpPurpose.VerifyEmail,
      });

      if (!result.isSuccess) {
        message.error(result.message || "Xác minh OTP thất bại");
        return;
      }

      message.success("Tài khoản đã được xác minh");
      navigate("/login", { replace: true });
    } finally {
      setSubmitting(false);
    }
  };

  const handleResendOtp = async () => {
    if (!registeredEmail) return;

    const result = await sendOtp({
      email: registeredEmail,
      purpose: OtpPurpose.VerifyEmail,
    });

    if (result.isSuccess) {
      message.success(result.message || "Đã gửi lại OTP");
    } else {
      message.error(result.message || "Không thể gửi OTP");
    }
  };

  return (
    <main className="register-page">
      <section className="register-page__panel">
        <div className="register-page__intro">
          <Title level={1}>Tạo tài khoản</Title>
          <Text className="register-page__subtitle">
            đăng ký và xác minh email để bắt đầu theo dõi công việc hằng ngày.
          </Text>
        </div>

        <img
          className="register-page__image"
          src={authIllustration}
          alt="Minh họa quản lý công việc"
        />

        <Card className="register-page__card" variant="borderless">
          <Steps
            current={step}
            items={[
              { title: "Thông tin" },
              { title: "Xác minh email" },
            ]}
            className="register-page__steps"
          />

          {step === 0 ? (
            <Form
              form={registerForm}
              className="register-page__form"
              layout="vertical"
              requiredMark={false}
              onFinish={handleRegister}
            >
              <div className="register-page__form-grid">
                <Form.Item
                  label="Họ tên"
                  name="name"
                  rules={[{ required: true, message: "Vui lòng nhập họ tên" }]}
                >
                  <Input prefix={<UserOutlined />} />
                </Form.Item>

                <Form.Item
                  label="Số điện thoại"
                  name="phoneNumber"
                  rules={[{ required: true, message: "Vui lòng nhập số điện thoại" }]}
                >
                  <Input prefix={<PhoneOutlined />} />
                </Form.Item>

                <Form.Item
                  className="register-page__form-field--wide"
                  label="Email"
                  name="email"
                  rules={[
                    { required: true, message: "Vui lòn nhập email" },
                    { type: "email", message: "Email không hợp lệ" },
                  ]}
                >
                  <Input prefix={<MailOutlined />} />
                </Form.Item>

                <Form.Item
                  label="Mật khẩu"
                  name="password"
                  rules={[
                    { required: true, message: "Vui lòng nhập mật khẩu" },
                    { min: 8, message: "Mật khẩu tối thiểu 8 ký tự" },
                  ]}
                >
                  <Input.Password
                    prefix={<LockOutlined />}
                    autoComplete="new-password"
                  />
                </Form.Item>

                <Form.Item
                  label="Xác nhận mật khẩu"
                  name="confirmPassword"
                  dependencies={["password"]}
                  rules={[
                    { required: true, message: "Vui lòng xác nhận mật khẩu" },
                    ({ getFieldValue }) => ({
                      validator(_, value: string | undefined) {
                        if (!value || getFieldValue("password") === value) {
                          return Promise.resolve();
                        }
                        return Promise.reject(new Error("Mật khẩu xác nhận không khớp"));
                      },
                    }),
                  ]}
                >
                  <Input.Password
                    prefix={<LockOutlined />}
                    autoComplete="new-password"
                  />
                </Form.Item>
              </div>

              <Button
                type="primary"
                htmlType="submit"
                icon={<UserAddOutlined />}
                loading={submitting}
                block
              >
                Đăng ký
              </Button>
            </Form>
          ) : (
            <Form
              form={otpForm}
              layout="vertical"
              requiredMark={false}
              onFinish={handleVerify}
            >
              <Text className="register-page__notice">
                Mã OTP đã được gửi đến {registeredEmail}.
              </Text>

              <Form.Item
                label="Mã OTP"
                name="code"
                rules={[
                  { required: true, message: "Vui lòng nhập OTP" },
                  { len: 6, message: "OTP gồm 6 ký tự" },
                ]}
              >
                <Input size="large" maxLength={6} inputMode="numeric" />
              </Form.Item>

              <Button
                type="primary"
                htmlType="submit"
                size="large"
                icon={<CheckCircleOutlined />}
                loading={submitting}
                block
              >
                Xác minh tài khoản
              </Button>

              <Button type="link" block onClick={handleResendOtp}>
                Gửi lại OTP
              </Button>
            </Form>
          )}

          <Space className="register-page__footer">
            <Text type="secondary">Đã có tài khoản?</Text>
            <Link to="/login">Đăng nhập</Link>
          </Space>
        </Card>
      </section>
    </main>
  );
};

export default Register;
