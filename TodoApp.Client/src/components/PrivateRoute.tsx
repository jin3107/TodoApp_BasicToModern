import { useEffect, useRef, useState } from "react";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import { Spin } from "antd";
import { refreshToken } from "../apis/authenticationAPI";

const PrivateRoute = () => {
  const location = useLocation();
  const hasVerified = useRef(false);
  const [status, setStatus] = useState<"checking" | "authenticated" | "unauthenticated">("checking");

  useEffect(() => {
    if (hasVerified.current) return;
    hasVerified.current = true;

    const verify = async () => {
      try {
        const res = await refreshToken();
        setStatus(res.isSuccess ? "authenticated" : "unauthenticated");
      } catch {
        setStatus("unauthenticated");
      }
    };
    verify();
  }, []);

  if (status === "checking") {
    return (
      <div className="route-loader">
        <Spin size="large" />
        <span>Đang kiểm tra phiên đăng nhập…</span>
      </div>
    );
  }

  return status === "authenticated" ? (
    <Outlet />
  ) : (
    <Navigate to="/login" replace state={{ from: location }} />
  );
};

export default PrivateRoute;
