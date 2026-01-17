export default interface SearchResponse<T> {
  data: {
    data: T[];
    currentPage: number;
    totalPages: number;
    rowsPerPage: number;
    totalRows: number
  },
  isSuccess: boolean;
  message: string;
}