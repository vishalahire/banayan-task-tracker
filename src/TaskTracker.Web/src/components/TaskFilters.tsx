import { TaskState, TaskPriority } from '../types';

interface TaskFiltersProps {
  search: string;
  onSearchChange: (search: string) => void;
  state: TaskState | '';
  onStateChange: (state: TaskState | '') => void;
  priority: TaskPriority | '';
  onPriorityChange: (priority: TaskPriority | '') => void;
  dueDateFrom: string;
  onDueDateFromChange: (date: string) => void;
  dueDateTo: string;
  onDueDateToChange: (date: string) => void;
  tags: string;
  onTagsChange: (tags: string) => void;
  onClearFilters: () => void;
}

export default function TaskFilters({
  search,
  onSearchChange,
  state,
  onStateChange,
  priority,
  onPriorityChange,
  dueDateFrom,
  onDueDateFromChange,
  dueDateTo,
  onDueDateToChange,
  tags,
  onTagsChange,
  onClearFilters,
}: TaskFiltersProps) {
  return (
    <div className="bg-white p-6 rounded-lg shadow mb-6">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {/* Search */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Search
          </label>
          <input
            type="text"
            value={search}
            onChange={(e) => onSearchChange(e.target.value)}
            placeholder="Search tasks..."
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* State */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Status
          </label>
          <select
            value={state}
            onChange={(e) => onStateChange(e.target.value as TaskState | '')}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Status</option>
            <option value={TaskState.New}>New</option>
            <option value={TaskState.InProgress}>In Progress</option>
            <option value={TaskState.Completed}>Completed</option>
            <option value={TaskState.Archived}>Archived</option>
          </select>
        </div>

        {/* Priority */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Priority
          </label>
          <select
            value={priority}
            onChange={(e) => onPriorityChange(e.target.value as TaskPriority | '')}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Priorities</option>
            <option value={TaskPriority.Low}>Low</option>
            <option value={TaskPriority.Medium}>Medium</option>
            <option value={TaskPriority.High}>High</option>
          </select>
        </div>

        {/* Due Date From */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Due Date From
          </label>
          <input
            type="date"
            value={dueDateFrom}
            onChange={(e) => onDueDateFromChange(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Due Date To */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Due Date To
          </label>
          <input
            type="date"
            value={dueDateTo}
            onChange={(e) => onDueDateToChange(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Tags */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Tags (comma separated)
          </label>
          <input
            type="text"
            value={tags}
            onChange={(e) => onTagsChange(e.target.value)}
            placeholder="urgent, work, personal"
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      {/* Clear Filters Button */}
      <div className="mt-4">
        <button
          onClick={onClearFilters}
          className="px-4 py-2 text-sm text-gray-600 hover:text-gray-800 border border-gray-300 rounded-md hover:bg-gray-50"
        >
          Clear Filters
        </button>
      </div>
    </div>
  );
}