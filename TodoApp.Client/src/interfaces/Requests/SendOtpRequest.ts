import type { OtpPurpose } from "../../commons/enums/OtpPupose";

export default interface SendOtpRequest {
  email: string;
  purpose: OtpPurpose
}
