import { apiClient } from './client';
import { AuditEvent } from '../types';

export const auditApi = {
  getAuditEventsForTask: (taskId: string): Promise<AuditEvent[]> => {
    return apiClient.get<AuditEvent[]>(`/api/tasks/${taskId}/audit-events`);
  },
};