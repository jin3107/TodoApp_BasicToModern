export interface TodoListResponse {
    id: string;
    name: string;
    description?: string;
    totalItems: number;
    completedItems: number;
    createdOn?: string;
    modifiedOn?: string;
}