import axios from "../configs/axios";
import type { AppResponse } from "../helpers";
import type {
  ChangePasswordRequest,
  LoginRequest,
  LoginResponse,
  RefreshTokenResponse,
  RegisterRequest,
  RegisterResponse,
  SendOtpRequest,
  SendOtpResponse,
  VerifyOtpRequest,
} from "../interfaces";
import type { ChangePasswordResponse } from "../interfaces/Responses";

const AUTHENTICATION_URL = "/authentication";

export const login = async (
  request: LoginRequest,
): Promise<AppResponse<LoginResponse>> => {
  const response = await axios.post<AppResponse<LoginResponse>>(
    `${AUTHENTICATION_URL}/login`,
    request,
  );
  return response.data;
};

export const register = async (
  request: RegisterRequest,
): Promise<AppResponse<RegisterResponse>> => {
  const response = await axios.post<AppResponse<RegisterResponse>>(
    `${AUTHENTICATION_URL}/register`,
    request,
  );
  return response.data;
};

export const sendOtp = async (
  request: SendOtpRequest,
): Promise<AppResponse<SendOtpResponse>> => {
  const response = await axios.post<AppResponse<SendOtpResponse>>(
    `${AUTHENTICATION_URL}/send-otp`,
    request,
  );
  return response.data;
};

export const verifyOtp = async (
  request: VerifyOtpRequest,
): Promise<AppResponse<boolean>> => {
  const response = await axios.post<AppResponse<boolean>>(
    `${AUTHENTICATION_URL}/verify-otp`,
    request,
  );
  return response.data;
};

export const changePassword = async (
  request: ChangePasswordRequest,
): Promise<AppResponse<ChangePasswordResponse>> => {
  const response = await axios.post<AppResponse<ChangePasswordResponse>>(
    `${AUTHENTICATION_URL}/change-password`,
    request,
  );
  return response.data;
};

export const refreshToken = async (): Promise<
  AppResponse<RefreshTokenResponse>
> => {
  const response = await axios.post<AppResponse<RefreshTokenResponse>>(
    `${AUTHENTICATION_URL}/refresh-token`,
  );
  return response.data;
};

export const logout = async (): Promise<{ message: string }> => {
  const response = await axios.post<{ message: string }>(
    `${AUTHENTICATION_URL}/logout`,
  );
  return response.data;
};
