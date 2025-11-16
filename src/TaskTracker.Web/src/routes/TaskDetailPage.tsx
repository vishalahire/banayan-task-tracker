import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { taskApi } from '../api/taskApi';
import { attachmentApi } from '../api/attachmentApi';
import { auditApi } from '../api/auditApi';
import { Task, TaskAttachment, AuditEvent, TaskState, TaskPriority } from '../types';
import AttachmentList from '../components/AttachmentList';
import AttachmentUpload from '../components/AttachmentUpload';
import Alert from '../components/Alert';

export default function TaskDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [task, setTask] = useState<Task | null>(null);
  const [attachments, setAttachments] = useState<TaskAttachment[]>([]);
  const [auditEvents, setAuditEvents] = useState<AuditEvent[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (id) {
      loadTaskDetails(id);
    }
  }, [id]);

  const loadTaskDetails = async (taskId: string) => {
    try {
      setIsLoading(true);
      setError('');

      const [taskData, attachmentsData, auditData] = await Promise.all([
        taskApi.getTaskById(taskId),
        attachmentApi.getAttachments(taskId),
        auditApi.getAuditEventsForTask(taskId).catch(() => []), // Optional audit data
      ]);

      setTask(taskData);
      setAttachments(attachmentsData);
      setAuditEvents(auditData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load task details');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAttachmentUpload = (attachment: TaskAttachment) => {
    setAttachments(prev => [...prev, attachment]);
  };

  const handleAttachmentDelete = (attachmentId: string) => {
    setAttachments(prev => prev.filter(a => a.id !== attachmentId));
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const getStateColor = (state: TaskState) => {
    switch (state) {
      case TaskState.New:
        return 'bg-gray-100 text-gray-800';
      case TaskState.InProgress:
        return 'bg-blue-100 text-blue-800';
      case TaskState.Completed:
        return 'bg-green-100 text-green-800';
      case TaskState.Archived:
        return 'bg-purple-100 text-purple-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getPriorityColor = (priority: TaskPriority) => {
    switch (priority) {
      case TaskPriority.Low:
        return 'bg-green-100 text-green-800';
      case TaskPriority.Medium:
        return 'bg-yellow-100 text-yellow-800';
      case TaskPriority.High:
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  if (isLoading) {
    return (
      <div className="text-center py-8">
        <div className="text-gray-500">Loading task details...</div>
      </div>
    );
  }

  if (!task) {
    return (
      <div className="text-center py-8">
        <div className="text-red-600">Task not found</div>
        <Link to="/tasks" className="text-blue-600 hover:text-blue-800">
          Back to tasks
        </Link>
      </div>
    );
  }

  const isOverdue = task.dueDate && new Date(task.dueDate) < new Date();

  return (
    <div className="max-w-4xl mx-auto">
      {error && <Alert type="error" message={error} onClose={() => setError('')} />}

      {/* Header */}
      <div className="flex justify-between items-start mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">{task.title}</h1>
          <div className="flex space-x-2">
            <Link
              to="/tasks"
              className="text-blue-600 hover:text-blue-800 text-sm"
            >
              ← Back to tasks
            </Link>
          </div>
        </div>
        <div className="flex space-x-2">
          <Link
            to={`/tasks/${task.id}/edit`}
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md text-sm font-medium"
          >
            Edit Task
          </Link>
        </div>
      </div>

      {/* Task Details */}
      <div className="bg-white rounded-lg shadow mb-6">
        <div className="p-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
            <div>
              <h3 className="text-sm font-medium text-gray-500 mb-2">Status</h3>
              <span className={`inline-flex px-3 py-1 rounded-full text-sm font-medium ${getStateColor(task.status)}`}>
                {task.status}
              </span>
            </div>
            <div>
              <h3 className="text-sm font-medium text-gray-500 mb-2">Priority</h3>
              <span className={`inline-flex px-3 py-1 rounded-full text-sm font-medium ${getPriorityColor(task.priority)}`}>
                {task.priority}
              </span>
            </div>
            {task.dueDate && (
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-2">Due Date</h3>
                <div className={isOverdue ? 'text-red-600 font-medium' : 'text-gray-900'}>
                  {formatDate(task.dueDate)}
                  {isOverdue && ' (Overdue)'}
                </div>
              </div>
            )}
            <div>
              <h3 className="text-sm font-medium text-gray-500 mb-2">Created</h3>
              <div className="text-gray-900">{formatDate(task.createdAt)}</div>
            </div>
          </div>

          {task.description && (
            <div className="mb-6">
              <h3 className="text-sm font-medium text-gray-500 mb-2">Description</h3>
              <div className="text-gray-900 whitespace-pre-wrap">{task.description}</div>
            </div>
          )}

          {task.tags.length > 0 && (
            <div className="mb-6">
              <h3 className="text-sm font-medium text-gray-500 mb-2">Tags</h3>
              <div className="flex flex-wrap gap-2">
                {task.tags.map((tag, index) => (
                  <span
                    key={index}
                    className="px-2 py-1 bg-gray-100 text-gray-700 text-sm rounded"
                  >
                    #{tag}
                  </span>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Attachments */}
      <div className="bg-white rounded-lg shadow mb-6">
        <div className="p-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Attachments</h3>
          
          <div className="mb-6">
            <AttachmentUpload
              taskId={task.id}
              onUploadComplete={handleAttachmentUpload}
            />
          </div>

          <AttachmentList
            taskId={task.id}
            attachments={attachments}
            onDelete={handleAttachmentDelete}
          />
        </div>
      </div>

      {/* Audit Events */}
      {auditEvents.length > 0 && (
        <div className="bg-white rounded-lg shadow">
          <div className="p-6">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Activity History</h3>
            <div className="space-y-4">
              {auditEvents.map((event) => (
                <div key={event.id} className="border-l-4 border-blue-200 pl-4 py-2">
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <div className="text-sm font-medium text-gray-900">
                        {event.action.replace(/([A-Z])/g, ' $1').trim()}
                      </div>
                      <div className="text-sm text-gray-700 mt-1">
                        {event.details}
                      </div>
                      <div className="text-xs text-gray-500 mt-1">
                        by {event.userDisplayName} • {formatDate(event.createdAt)}
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}