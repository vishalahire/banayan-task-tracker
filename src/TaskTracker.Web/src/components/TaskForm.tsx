import { useState } from 'react';
import { CreateTaskCommand, UpdateTaskCommand, TaskPriority, TaskState } from '../types';

interface TaskFormProps {
  initialData?: Partial<UpdateTaskCommand & { id: string; ownerUserId?: string }>;
  onSubmit: (data: CreateTaskCommand | UpdateTaskCommand, statusChanged?: { taskId: string; newStatus: TaskState }) => Promise<void>;
  isLoading: boolean;
  submitLabel: string;
  canEdit?: boolean;
}

export default function TaskForm({ initialData, onSubmit, isLoading, submitLabel, canEdit = true }: TaskFormProps) {
  const [formData, setFormData] = useState({
    title: initialData?.title || '',
    description: initialData?.description || '',
    priority: initialData?.priority || TaskPriority.Medium,
    status: initialData?.status || TaskState.New,
    dueDate: initialData?.dueDate ? initialData.dueDate.split('T')[0] : '',
    tags: initialData?.tags?.join(', ') || '',
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    const data: CreateTaskCommand | UpdateTaskCommand = {
      title: formData.title,
      description: formData.description || undefined,
      priority: formData.priority,
      dueDate: formData.dueDate || undefined,
      tags: formData.tags
        ? formData.tags.split(',').map(tag => tag.trim()).filter(tag => tag)
        : [],
    };

    // Check if status changed for update operations
    let statusChanged;
    if (initialData?.id && initialData?.status !== formData.status) {
      statusChanged = {
        taskId: initialData.id,
        newStatus: formData.status
      };
    }

    await onSubmit(data, statusChanged);
  };

  const handleChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Permission message for non-owners */}
      {initialData?.id && !canEdit && (
        <div className="bg-yellow-50 border border-yellow-200 rounded-md p-4 mb-4">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg className="h-5 w-5 text-yellow-400" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
              </svg>
            </div>
            <div className="ml-3">
              <h3 className="text-sm font-medium text-yellow-800">
                Read-only access
              </h3>
              <p className="mt-1 text-sm text-yellow-700">
                You can view this task but cannot edit it because you are not the owner.
              </p>
            </div>
          </div>
        </div>
      )}
      
      {/* Title */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Title *
        </label>
        <input
          type="text"
          required
          value={formData.title}
          onChange={(e) => handleChange('title', e.target.value)}
          disabled={!canEdit}
          className={`w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${!canEdit ? 'bg-gray-100 text-gray-500' : ''}`}
          placeholder="Enter task title"
        />
      </div>

      {/* Description */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Description
        </label>
        <textarea
          rows={4}
          value={formData.description}
          onChange={(e) => handleChange('description', e.target.value)}
          disabled={!canEdit}
          className={`w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${!canEdit ? 'bg-gray-100 text-gray-500' : ''}`}
          placeholder="Enter task description"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Priority */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Priority *
          </label>
          <select
            required
            value={formData.priority}
            onChange={(e) => handleChange('priority', e.target.value)}
            disabled={!canEdit}
            className={`w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${!canEdit ? 'bg-gray-100 text-gray-500' : ''}`}
          >
            <option value={TaskPriority.Low}>Low</option>
            <option value={TaskPriority.Medium}>Medium</option>
            <option value={TaskPriority.High}>High</option>
          </select>
        </div>

        {/* State (only show for edit mode) */}
        {initialData?.id && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Status *
            </label>
            <select
              required
              value={formData.status}
              onChange={(e) => handleChange('status', e.target.value)}
              disabled={!canEdit}
              className={`w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${!canEdit ? 'bg-gray-100 text-gray-500' : ''}`}
            >
              <option value={TaskState.New}>New</option>
              <option value={TaskState.InProgress}>In Progress</option>
              <option value={TaskState.Completed}>Completed</option>
              <option value={TaskState.Archived}>Archived</option>
            </select>
          </div>
        )}

        {/* Due Date */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Due Date
          </label>
          <input
            type="date"
            value={formData.dueDate}
            onChange={(e) => handleChange('dueDate', e.target.value)}
            disabled={!canEdit}
            className={`w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${!canEdit ? 'bg-gray-100 text-gray-500' : ''}`}
          />
        </div>

        {/* Tags */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Tags (comma separated)
          </label>
          <input
            type="text"
            value={formData.tags}
            onChange={(e) => handleChange('tags', e.target.value)}
            disabled={!canEdit}
            className={`w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${!canEdit ? 'bg-gray-100 text-gray-500' : ''}`}
            placeholder="urgent, work, personal"
          />
        </div>
      </div>

      {/* Submit Button */}
      <div className="flex justify-end space-x-3">
        <button
          type="button"
          onClick={() => window.history.back()}
          className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
        >
          {canEdit ? 'Cancel' : 'Back'}
        </button>
        {canEdit && (
          <button
            type="submit"
            disabled={isLoading}
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
          >
            {isLoading ? 'Saving...' : submitLabel}
          </button>
        )}
      </div>
    </form>
  );
}