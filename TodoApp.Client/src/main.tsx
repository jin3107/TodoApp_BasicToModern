import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { ConfigProvider, App as AntdApp } from "antd";
import viVN from "antd/locale/vi_VN";
import dayjs from "dayjs";
import "dayjs/locale/vi";
import "@ant-design/v5-patch-for-react-19";
import "./scss/global.scss";
import App from "./App.tsx";

dayjs.locale("vi");

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ConfigProvider
      locale={viVN}
      theme={{
        token: {
          colorPrimary: "#b68235",
          colorPrimaryHover: "#e1ad66",
          colorPrimaryActive: "#7d5411",
          colorLink: "#b68235",
          colorText: "#201f1d",
          colorBorder: "rgba(32, 31, 29, 0.16)",
          colorBorderSecondary: "rgba(32, 31, 29, 0.16)",
          colorBgLayout: "#f3f2f2",
          colorBgContainer: "#ffffff",
          borderRadius: 4,
          borderRadiusSM: 2,
          borderRadiusLG: 7,
          fontFamily:
            '"Lora", -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial',
        },
        components: {
          Button: { fontFamily: '"Cormorant Garamond", sans-serif', fontWeight: 600 },
          Layout: {
            headerBg: "#1a1817",
            siderBg: "#1a1817",
          },
        },
      }}
    >
      <AntdApp>
        <App />
      </AntdApp>
    </ConfigProvider>
  </StrictMode>
);
