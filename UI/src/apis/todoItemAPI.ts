import type { AppResponse } from "../helpers";
import type { SearchRequest, SearchResponse, TodoItemRequest, TodoItemResponse } from "../interfaces";
import axios from '../configs/axios'

export const createTodoItem = async (data: TodoItemRequest) : Promise<AppResponse<TodoItemResponse>> => {
  const response = await axios.post<AppResponse<TodoItemResponse>>("/todo-items", data);
  return response.data;
}

export const updateTodoItem = async (data: TodoItemRequest) : Promise<AppResponse<TodoItemResponse>> => {
  const response = await axios.put<AppResponse<TodoItemResponse>>("/todo-items", data);
  return response.data;
}

export const getTodoItemById= async (id: string) : Promise<AppResponse<TodoItemResponse>> => {
  const response = await axios.get<AppResponse<TodoItemResponse>>(`/todo-items/${id}`);
  return response.data;
}

export const deleteTodoItem = async (id: string) : Promise<AppResponse<string>> => {
  const response = await axios.delete<AppResponse<string>>(`/todo-items/${id}`);
  return response.data;
}

export const searchTodoItems = async (searchRequest: SearchRequest) : Promise<AppResponse<SearchResponse<TodoItemResponse>>> => {
  const response = await axios.post<AppResponse<SearchResponse<TodoItemResponse>>>("/todo-items/search", searchRequest);
  return response.data;
};