import { apiClient } from './client';
import { TaskAttachment } from '../types';

export const attachmentApi = {
  getAttachments: (taskId: string): Promise<TaskAttachment[]> => {
    return apiClient.get<TaskAttachment[]>(`/api/attachments/task/${taskId}`);
  },

  uploadAttachment: (taskId: string, file: File): Promise<TaskAttachment> => {
    const formData = new FormData();
    formData.append('file', file);
    return apiClient.postFormData<TaskAttachment>(`/api/attachments/task/${taskId}`, formData);
  },

  deleteAttachment: (attachmentId: string): Promise<void> => {
    return apiClient.delete<void>(`/api/attachments/${attachmentId}`);
  },

  downloadAttachment: (attachmentId: string): string => {
    const token = localStorage.getItem('tasktracker_token');
    const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
    const url = `${baseUrl}/api/attachments/${attachmentId}/download`;
    
    if (token) {
      return `${url}?token=${encodeURIComponent(token)}`;
    }
    
    return url;
  },
};