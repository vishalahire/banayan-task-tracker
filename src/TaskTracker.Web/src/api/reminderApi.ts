import { apiClient } from './client';

export interface PendingReminder {
  taskId: string;
  taskTitle: string;
  dueDate: string;
  ownerUserId: string;
  ownerEmail: string;
  ownerDisplayName: string;
  reminderType: string;
  hasReminderBeenSent: boolean;
  hoursUntilDue: number;
}

export interface ReminderProcessingResult {
  totalPending: number;
  processedCount: number;
  successfulCount: number;
  failedCount: number;
  skippedCount: number;
  processedTaskIds: string[];
  errors: string[];
}

export const reminderApi = {
  getPendingReminders: (): Promise<PendingReminder[]> => {
    return apiClient.get<PendingReminder[]>('/api/reminders/pending');
  },

  processPendingReminders: (): Promise<ReminderProcessingResult> => {
    return apiClient.post<ReminderProcessingResult>('/api/reminders/process', {});
  },
};