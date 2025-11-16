import { apiClient } from './client';
import { LoginRequest, LoginResponse, RegisterRequest, ChangePasswordRequest } from '../types';

export const authApi = {
  login: (credentials: LoginRequest): Promise<LoginResponse> => {
    return apiClient.post<LoginResponse>('/api/auth/login', credentials);
  },

  register: (userData: RegisterRequest): Promise<LoginResponse> => {
    return apiClient.post<LoginResponse>('/api/auth/register', userData);
  },

  changePassword: (request: ChangePasswordRequest): Promise<void> => {
    return apiClient.post<void>('/api/auth/change-password', request);
  },
};