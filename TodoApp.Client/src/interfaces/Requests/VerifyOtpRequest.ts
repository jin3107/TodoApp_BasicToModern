import type { OtpPurpose } from "../../commons/enums/OtpPupose";

export default interface VerifyOtpRequest {
  email: string;
  code: string;
  purpose: OtpPurpose
}
