export interface TodoListRequest {
    id?: string; // Make id optional for create requests
    name: string;
    description?: string;
}