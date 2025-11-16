import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import LoginPage from '../routes/LoginPage';

// Mock the useAuth hook
vi.mock('../hooks/useAuth', () => ({
  useAuth: () => ({
    login: vi.fn(),
    isLoading: false,
    error: ''
  })
}));

describe('LoginPage', () => {
  const renderLoginPage = () => {
    render(
      <BrowserRouter>
        <LoginPage />
      </BrowserRouter>
    );
  };

  test('renders email and password fields and login button', () => {
    // Act
    renderLoginPage();

    // Assert
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
  });

  test('renders sign up link', () => {
    // Act
    renderLoginPage();

    // Assert
    expect(screen.getByText(/or/i, { selector: 'p' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /create a new account/i })).toBeInTheDocument();
  });

  test('renders form with correct structure', () => {
    // Act
    renderLoginPage();

    // Assert
    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/password/i);
    
    expect(emailInput).toHaveAttribute('type', 'email');
    expect(passwordInput).toHaveAttribute('type', 'password');
    expect(emailInput).toBeRequired();
    expect(passwordInput).toBeRequired();
    
    // Check that inputs are inside a form element
    const form = emailInput.closest('form');
    expect(form).toBeInTheDocument();
  });
});