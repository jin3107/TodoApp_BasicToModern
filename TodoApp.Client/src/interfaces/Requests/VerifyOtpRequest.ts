import type { OtpPurpose } from "../../commons";

export default interface VerifyOtpRequest {
  email: string;
  code: string;
  purpose: OtpPurpose
}