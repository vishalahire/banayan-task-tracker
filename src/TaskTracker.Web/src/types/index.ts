// API response types based on our backend DTOs
export interface User {
  id: string;
  email: string;
  displayName: string;
  createdAt: string;
}

export interface Task {
  id: string;
  title: string;
  description?: string;
  status: TaskState;
  priority: TaskPriority;
  dueDate?: string;
  tags: string[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskCommand {
  title: string;
  description?: string;
  priority: TaskPriority;
  dueDate?: string;
  tags: string[];
}

export interface UpdateTaskCommand {
  title?: string;
  description?: string;
  status?: TaskState;
  priority?: TaskPriority;
  dueDate?: string;
  tags?: string[];
}

export interface TaskAttachment {
  id: string;
  taskId: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  createdAt: string;
}

export interface AuditEvent {
  id: string;
  entityId: string;
  entityType: string;
  action: string;
  details: string;
  userId: string;
  userDisplayName: string;
  createdAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface TasksResponse {
  items: Task[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export enum TaskState {
  New = 'New',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Archived = 'Archived'
}

export enum TaskPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High'
}

export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
}