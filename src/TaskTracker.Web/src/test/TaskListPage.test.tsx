import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import TaskListPage from '../routes/TaskListPage';

// Mock the API and auth hooks
vi.mock('../api/taskApi', () => ({
  taskApi: {
    getTasks: vi.fn(() => Promise.resolve({
      items: [
        {
          id: '1',
          title: 'Test Task 1',
          status: 'New',
          priority: 'Medium',
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          tags: ['test']
        },
        {
          id: '2', 
          title: 'Test Task 2',
          status: 'InProgress',
          priority: 'High',
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          tags: ['urgent']
        }
      ],
      totalCount: 2,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false
    })),
    deleteTask: vi.fn()
  }
}));

vi.mock('../hooks/useAuth', () => ({
  useAuth: () => ({
    currentUser: { email: 'test@example.com' },
    isAuthenticated: true
  })
}));

describe('TaskListPage', () => {
  const renderTaskListPage = () => {
    render(
      <BrowserRouter>
        <TaskListPage />
      </BrowserRouter>
    );
  };

  test('renders page title and new task button', async () => {
    // Act
    renderTaskListPage();

    // Assert
    expect(screen.getByText('My Tasks')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /new task/i })).toBeInTheDocument();
  });

  test('renders task titles when tasks are loaded', async () => {
    // Act
    renderTaskListPage();

    // Assert - Wait for tasks to load and check if titles appear
    expect(await screen.findByText('Test Task 1')).toBeInTheDocument();
    expect(await screen.findByText('Test Task 2')).toBeInTheDocument();
  });

  test('renders filters section', async () => {
    // Act
    renderTaskListPage();

    // Assert - Use placeholder text and roles since labels aren't properly associated
    expect(await screen.findByPlaceholderText(/search tasks/i)).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();
    expect(screen.getByText('Priority')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /clear filters/i })).toBeInTheDocument();
  });

  test('renders results summary', async () => {
    // Act
    renderTaskListPage();

    // Assert
    expect(await screen.findByText(/showing 2 of 2 tasks/i)).toBeInTheDocument();
  });
});