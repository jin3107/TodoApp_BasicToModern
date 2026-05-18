export default interface LoginResponse {
  userName: string;
  email: string;
  phoneNumber: string;
  role: string;
  accessToken: string;
  refreshToken: string;
}