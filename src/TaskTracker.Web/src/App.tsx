import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './hooks/useAuth';
import Layout from './components/Layout';
import LoginPage from './routes/LoginPage';
import RegisterPage from './routes/RegisterPage';
import TaskListPage from './routes/TaskListPage';
import TaskDetailPage from './routes/TaskDetailPage';
import TaskEditPage from './routes/TaskEditPage';
import ChangePasswordPage from './routes/ChangePasswordPage';

function AppRoutes() {
  const { isAuthenticated } = useAuth();

  return (
    <Routes>
      <Route path="/login" element={isAuthenticated ? <Navigate to="/tasks" replace /> : <LoginPage />} />
      <Route path="/register" element={isAuthenticated ? <Navigate to="/tasks" replace /> : <RegisterPage />} />
      
      {isAuthenticated ? (
        <Route path="/" element={<Layout />}>
          <Route path="/" element={<Navigate to="/tasks" replace />} />
          <Route path="/tasks" element={<TaskListPage />} />
          <Route path="/tasks/new" element={<TaskEditPage />} />
          <Route path="/tasks/:id" element={<TaskDetailPage />} />
          <Route path="/tasks/:id/edit" element={<TaskEditPage />} />
          <Route path="/change-password" element={<ChangePasswordPage />} />
        </Route>
      ) : (
        <Route path="*" element={<Navigate to="/login" replace />} />
      )}
    </Routes>
  );
}

function App() {
  return (
    <AuthProvider>
      <Router>
        <AppRoutes />
      </Router>
    </AuthProvider>
  );
}

export default App;