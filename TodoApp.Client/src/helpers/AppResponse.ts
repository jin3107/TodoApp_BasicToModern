export default interface AppResponse<T> {
  isSuccess: boolean;
  message: string;
  data: T;
}
