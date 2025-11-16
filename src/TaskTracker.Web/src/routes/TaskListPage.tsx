import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { taskApi, TaskFilters as TaskFiltersType } from '../api/taskApi';
import { Task, TaskState, TaskPriority } from '../types';
import TaskCard from '../components/TaskCard';
import TaskFilters from '../components/TaskFilters';
import Alert from '../components/Alert';

export default function TaskListPage() {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);

  // Filter state
  const [search, setSearch] = useState('');
  const [state, setState] = useState<TaskState | ''>('');
  const [priority, setPriority] = useState<TaskPriority | ''>('');
  const [dueDateFrom, setDueDateFrom] = useState('');
  const [dueDateTo, setDueDateTo] = useState('');
  const [tags, setTags] = useState('');

  const loadTasks = async () => {
    try {
      setIsLoading(true);
      setError('');
      
      const filters: TaskFiltersType = {
        page: currentPage,
        pageSize,
      };

      if (search) filters.search = search;
      if (state) filters.state = state;
      if (priority) filters.priority = priority;
      if (dueDateFrom) filters.dueDateFrom = dueDateFrom;
      if (dueDateTo) filters.dueDateTo = dueDateTo;
      if (tags) filters.tags = tags.split(',').map(t => t.trim()).filter(t => t);

      const response = await taskApi.getTasks(filters);
      setTasks(response.items);
      setTotalCount(response.totalCount);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load tasks');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadTasks();
  }, [currentPage, search, state, priority, dueDateFrom, dueDateTo, tags]);

  const handleDelete = async (taskId: string) => {
    if (!window.confirm('Are you sure you want to delete this task?')) {
      return;
    }

    try {
      await taskApi.deleteTask(taskId);
      await loadTasks(); // Reload tasks
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete task');
    }
  };

  const clearFilters = () => {
    setSearch('');
    setState('');
    setPriority('');
    setDueDateFrom('');
    setDueDateTo('');
    setTags('');
    setCurrentPage(1);
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div>
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-900">My Tasks</h1>
        <Link
          to="/tasks/new"
          className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md font-medium"
        >
          New Task
        </Link>
      </div>

      {error && <Alert type="error" message={error} onClose={() => setError('')} />}

      {/* Filters */}
      <TaskFilters
        search={search}
        onSearchChange={setSearch}
        state={state}
        onStateChange={setState}
        priority={priority}
        onPriorityChange={setPriority}
        dueDateFrom={dueDateFrom}
        onDueDateFromChange={setDueDateFrom}
        dueDateTo={dueDateTo}
        onDueDateToChange={setDueDateTo}
        tags={tags}
        onTagsChange={setTags}
        onClearFilters={clearFilters}
      />

      {/* Loading */}
      {isLoading ? (
        <div className="text-center py-8">
          <div className="text-gray-500">Loading tasks...</div>
        </div>
      ) : (
        <>
          {/* Results summary */}
          <div className="mb-4 text-sm text-gray-600">
            Showing {tasks.length} of {totalCount} tasks
          </div>

          {/* Tasks grid */}
          {tasks.length === 0 ? (
            <div className="text-center py-8">
              <div className="text-gray-500 mb-4">No tasks found</div>
              <Link
                to="/tasks/new"
                className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md font-medium"
              >
                Create your first task
              </Link>
            </div>
          ) : (
            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
              {tasks.map((task) => (
                <TaskCard key={task.id} task={task} onDelete={handleDelete} />
              ))}
            </div>
          )}

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex justify-center mt-8">
              <nav className="flex space-x-2">
                <button
                  onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
                  disabled={currentPage === 1}
                  className="px-3 py-2 text-sm border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50"
                >
                  Previous
                </button>
                
                {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
                  <button
                    key={page}
                    onClick={() => setCurrentPage(page)}
                    className={`px-3 py-2 text-sm border rounded-md ${
                      currentPage === page
                        ? 'bg-blue-600 text-white border-blue-600'
                        : 'border-gray-300 hover:bg-gray-50'
                    }`}
                  >
                    {page}
                  </button>
                ))}
                
                <button
                  onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
                  disabled={currentPage === totalPages}
                  className="px-3 py-2 text-sm border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50"
                >
                  Next
                </button>
              </nav>
            </div>
          )}
        </>
      )}
    </div>
  );
}