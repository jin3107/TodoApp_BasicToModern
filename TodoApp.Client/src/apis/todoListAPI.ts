import axios from "../configs/axios";
import type { AppResponse } from "../helpers";
import type { TodoListRequest, TodoListResponse, SearchRequest, SearchResponse } from "../interfaces";

export const searchTodoLists = async (searchRequest: SearchRequest): Promise<AppResponse<SearchResponse<TodoListResponse>>> => {
  const response = await axios.post<AppResponse<SearchResponse<TodoListResponse>>>("/todo-lists/search", searchRequest);
  return response.data;
};

export const getTodoListById = async (id: string): Promise<AppResponse<TodoListResponse>> => {
  const response = await axios.get<AppResponse<TodoListResponse>>(`/todo-lists/${id}`);
  return response.data;
};

export const createTodoList = async (request: TodoListRequest): Promise<AppResponse<TodoListResponse>> => {
  console.log('Creating todo list with request:', request);
  const response = await axios.post<AppResponse<TodoListResponse>>("/todo-lists", request);
  return response.data;
};

export const updateTodoList = async (request: TodoListRequest): Promise<AppResponse<TodoListResponse>> => {
  const response = await axios.put<AppResponse<TodoListResponse>>("/todo-lists", request);
  return response.data;
};

export const deleteTodoList = async (id: string): Promise<AppResponse<string>> => {
  const response = await axios.delete<AppResponse<string>>(`/todo-lists/${id}`);
  return response.data;
};