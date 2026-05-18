export default interface ChangePasswordRequest {
  email: string;
  newPassword: string;
  confirmNewPassword: string;
}