import { apiClient } from './client';
import { Task, CreateTaskCommand, UpdateTaskCommand, TasksResponse, TaskState, TaskPriority } from '../types';

export interface TaskFilters {
  search?: string;
  state?: TaskState;
  priority?: TaskPriority;
  dueDateFrom?: string;
  dueDateTo?: string;
  tags?: string[];
  page?: number;
  pageSize?: number;
}

export const taskApi = {
  getTasks: (filters: TaskFilters = {}): Promise<TasksResponse> => {
    const params = new URLSearchParams();
    
    if (filters.search) params.append('searchText', filters.search);
    if (filters.state) params.append('status', filters.state);
    if (filters.priority) params.append('priority', filters.priority);
    if (filters.dueDateFrom) params.append('dueDateFrom', filters.dueDateFrom);
    if (filters.dueDateTo) params.append('dueDateTo', filters.dueDateTo);
    if (filters.tags?.length) {
      filters.tags.forEach(tag => params.append('tags', tag));
    }
    if (filters.page) params.append('pageNumber', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());

    const queryString = params.toString();
    return apiClient.get<TasksResponse>(`/api/tasks${queryString ? `?${queryString}` : ''}`);
  },

  getTaskById: (id: string): Promise<Task> => {
    return apiClient.get<Task>(`/api/tasks/${id}`);
  },

  createTask: (command: CreateTaskCommand): Promise<Task> => {
    return apiClient.post<Task>('/api/tasks', command);
  },

  updateTask: (id: string, command: UpdateTaskCommand): Promise<Task> => {
    return apiClient.put<Task>(`/api/tasks/${id}`, command);
  },

  updateTaskStatus: (id: string, status: TaskState): Promise<void> => {
    return apiClient.patch<void>(`/api/tasks/${id}/status`, { status });
  },

  deleteTask: (id: string): Promise<void> => {
    return apiClient.delete<void>(`/api/tasks/${id}`);
  },
};