import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { taskApi } from '../api/taskApi';
import { CreateTaskCommand, UpdateTaskCommand, TaskState } from '../types';
import { useAuth } from '../hooks/useAuth';
import TaskForm from '../components/TaskForm';
import Alert from '../components/Alert';

export default function TaskEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { currentUser } = useAuth();
  const isEdit = Boolean(id);

  const [task, setTask] = useState<any>(null);
  
  const canEdit = !isEdit || (task && currentUser && task.ownerUserId === currentUser.id);
  const [isLoading, setIsLoading] = useState(isEdit);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (isEdit && id) {
      loadTask(id);
    }
  }, [isEdit, id]);

  const loadTask = async (taskId: string) => {
    try {
      setIsLoading(true);
      const taskData = await taskApi.getTaskById(taskId);
      setTask(taskData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load task');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (
    data: CreateTaskCommand | UpdateTaskCommand, 
    statusChanged?: { taskId: string; newStatus: TaskState }
  ) => {
    try {
      setIsSaving(true);
      setError('');

      if (isEdit && id) {
        // Update the main task data
        await taskApi.updateTask(id, data as UpdateTaskCommand);
        
        // Update status separately if it changed
        if (statusChanged) {
          await taskApi.updateTaskStatus(statusChanged.taskId, statusChanged.newStatus);
        }
      } else {
        await taskApi.createTask(data as CreateTaskCommand);
      }

      navigate('/tasks');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save task');
    } finally {
      setIsSaving(false);
    }
  };

  if (isLoading) {
    return (
      <div className="text-center py-8">
        <div className="text-gray-500">Loading task...</div>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">
          {isEdit ? 'Edit Task' : 'Create New Task'}
        </h1>
      </div>

      {error && <Alert type="error" message={error} onClose={() => setError('')} />}

      <div className="bg-white rounded-lg shadow p-6">
        <TaskForm
          initialData={task}
          onSubmit={handleSubmit}
          isLoading={isSaving}
          submitLabel={isEdit ? 'Update Task' : 'Create Task'}
          canEdit={canEdit}
        />
      </div>
    </div>
  );
}