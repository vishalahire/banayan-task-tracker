import { apiClient } from './client';
import { TaskAttachment } from '../types';

export const attachmentApi = {
  getAttachments: (taskId: string): Promise<TaskAttachment[]> => {
    return apiClient.get<TaskAttachment[]>(`/api/tasks/${taskId}/attachments`);
  },

  uploadAttachment: (taskId: string, file: File): Promise<TaskAttachment> => {
    const formData = new FormData();
    formData.append('file', file);
    return apiClient.postFormData<TaskAttachment>(`/api/tasks/${taskId}/attachments`, formData);
  },

  deleteAttachment: (taskId: string, attachmentId: string): Promise<void> => {
    return apiClient.delete<void>(`/api/tasks/${taskId}/attachments/${attachmentId}`);
  },

  downloadAttachment: async (taskId: string, attachmentId: string): Promise<void> => {
    const token = localStorage.getItem('tasktracker_token');
    const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
    const url = `${baseUrl}/api/tasks/${taskId}/attachments/${attachmentId}`;
    
    try {
      const response = await fetch(url, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      
      // Get the filename from the Content-Disposition header or use a default
      const contentDisposition = response.headers.get('Content-Disposition');
      let filename = `attachment_${attachmentId}`;
      
      if (contentDisposition) {
        const filenameMatch = contentDisposition.match(/filename="?(.+)"?/);
        if (filenameMatch) {
          filename = filenameMatch[1];
        }
      }
      
      // Convert response to blob and create download
      const blob = await response.blob();
      const downloadUrl = window.URL.createObjectURL(blob);
      
      // Create temporary link and trigger download
      const link = document.createElement('a');
      link.href = downloadUrl;
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      
      // Clean up the blob URL
      window.URL.revokeObjectURL(downloadUrl);
    } catch (error) {
      console.error('Download failed:', error);
      throw new Error('Failed to download attachment');
    }
  },
};