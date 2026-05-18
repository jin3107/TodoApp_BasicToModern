import type { OtpPurpose } from "../../commons";

export default interface SendOtpRequest {
  email: string;
  purpose: OtpPurpose
}