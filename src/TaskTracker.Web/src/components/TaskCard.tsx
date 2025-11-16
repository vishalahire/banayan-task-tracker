import { Link } from 'react-router-dom';
import { Task, TaskState, TaskPriority } from '../types';

interface TaskCardProps {
  task: Task;
  onDelete: (taskId: string) => void;
}

export default function TaskCard({ task, onDelete }: TaskCardProps) {
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const getStateColor = (state: TaskState) => {
    switch (state) {
      case TaskState.Todo:
        return 'bg-gray-100 text-gray-800';
      case TaskState.InProgress:
        return 'bg-blue-100 text-blue-800';
      case TaskState.Done:
        return 'bg-green-100 text-green-800';
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

  const isOverdue = task.dueDate && new Date(task.dueDate) < new Date();

  return (
    <div className={`bg-white rounded-lg shadow p-6 ${isOverdue ? 'border-l-4 border-red-500' : ''}`}>
      <div className="flex justify-between items-start mb-3">
        <h3 className="text-lg font-semibold text-gray-900">
          <Link to={`/tasks/${task.id}`} className="hover:text-blue-600">
            {task.title}
          </Link>
        </h3>
        <div className="flex space-x-2">
          <Link
            to={`/tasks/${task.id}/edit`}
            className="text-blue-600 hover:text-blue-800 text-sm"
          >
            Edit
          </Link>
          <button
            onClick={() => onDelete(task.id)}
            className="text-red-600 hover:text-red-800 text-sm"
          >
            Delete
          </button>
        </div>
      </div>

      {task.description && (
        <p className="text-gray-600 mb-3 line-clamp-2">{task.description}</p>
      )}

      <div className="flex flex-wrap gap-2 mb-3">
        <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStateColor(task.state)}`}>
          {task.state}
        </span>
        <span className={`px-2 py-1 rounded-full text-xs font-medium ${getPriorityColor(task.priority)}`}>
          {task.priority}
        </span>
      </div>

      {task.tags.length > 0 && (
        <div className="flex flex-wrap gap-1 mb-3">
          {task.tags.map((tag, index) => (
            <span
              key={index}
              className="px-2 py-1 bg-gray-100 text-gray-700 text-xs rounded"
            >
              #{tag}
            </span>
          ))}
        </div>
      )}

      <div className="text-sm text-gray-500 space-y-1">
        {task.dueDate && (
          <div className={isOverdue ? 'text-red-600 font-medium' : ''}>
            Due: {formatDate(task.dueDate)}
            {isOverdue && ' (Overdue)'}
          </div>
        )}
        <div>Created: {formatDate(task.createdAt)}</div>
      </div>
    </div>
  );
}