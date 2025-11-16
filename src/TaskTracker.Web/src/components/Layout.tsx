import { Outlet, Link } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export default function Layout() {
  const { currentUser, logout } = useAuth();

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            {/* Left side - App title and nav */}
            <div className="flex items-center">
              <Link to="/tasks" className="text-xl font-bold text-gray-900 mr-8">
                Task Tracker
              </Link>
              <nav className="hidden md:flex space-x-6">
                <Link 
                  to="/tasks" 
                  className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium"
                >
                  Tasks
                </Link>
                <Link 
                  to="/tasks/new" 
                  className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium"
                >
                  New Task
                </Link>
              </nav>
            </div>

            {/* Right side - User info and logout */}
            <div className="flex items-center space-x-4">
              {currentUser && (
                <span className="text-sm text-gray-700">
                  Logged in as {currentUser.email}
                </span>
              )}
              <Link
                to="/change-password"
                className="text-gray-600 hover:text-gray-900 text-sm font-medium"
              >
                Change Password
              </Link>
              <button
                onClick={logout}
                className="bg-gray-600 hover:bg-gray-700 text-white px-4 py-2 rounded-md text-sm font-medium"
              >
                Logout
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Main content */}
      <main className="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
        <Outlet />
      </main>
    </div>
  );
}