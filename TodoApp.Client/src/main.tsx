import { StrictMode, useEffect } from "react";
import { createRoot } from "react-dom/client";
import { ConfigProvider, App as AntdApp } from "antd";
import viVN from "antd/locale/vi_VN";
import dayjs from "dayjs";
import "dayjs/locale/vi";
import "@ant-design/v5-patch-for-react-19";
import "./scss/global.scss";
import App from "./App.tsx";
import { ThemeProvider, useAppTheme } from "./commons/ThemeContext";
import { getClassicalTheme } from "./commons/classicalTheme";

dayjs.locale("vi");

const ThemedApp = () => {
  const { theme } = useAppTheme();

  useEffect(() => {
    document.documentElement.dataset.theme = theme;
  }, [theme]);

  return (
    <ConfigProvider locale={viVN} theme={getClassicalTheme(theme === "dark")}>
      <AntdApp>
        <App />
      </AntdApp>
    </ConfigProvider>
  );
};

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ThemeProvider>
      <ThemedApp />
    </ThemeProvider>
  </StrictMode>
);
