import { useState } from 'react';
import type { FormEvent } from 'react';
import { login, register } from '../../api/authApi';
import { setToken } from '../../api/client';
import './Auth.css';

type Mode = 'login' | 'register';

interface AuthProps {
  onAuthenticated: () => void;
}

function Auth({ onAuthenticated }: AuthProps) {
  const [mode, setMode] = useState<Mode>('login');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      const auth = mode === 'login' ? await login(email, password) : await register(email, password);
      setToken(auth.token);
      onAuthenticated();
    } catch (err) {
      const status = (err as { response?: { status?: number; data?: { message?: string } } }).response?.status;
      if (status === 401) {
        setError('Invalid email or password.');
      } else if (status === 409) {
        setError('An account with this email already exists.');
      } else if (status === 400) {
        setError('Password must be at least 8 characters long.');
      } else {
        setError('Something went wrong. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="auth">
      <div className="auth__card">
        <h1 className="auth__title">WebExplain</h1>
        <div className="auth__mode-switch">
          <button
            type="button"
            className={`auth__mode-button ${mode === 'login' ? 'auth__mode-button--active' : ''}`}
            onClick={() => setMode('login')}
          >
            Log in
          </button>
          <button
            type="button"
            className={`auth__mode-button ${mode === 'register' ? 'auth__mode-button--active' : ''}`}
            onClick={() => setMode('register')}
          >
            Register
          </button>
        </div>

        <form className="auth__form" onSubmit={handleSubmit}>
          <input
            type="email"
            className="auth__input"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            autoComplete="email"
            required
          />
          <input
            type="password"
            className="auth__input"
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
            minLength={mode === 'register' ? 8 : undefined}
            required
          />
          {error && <p className="auth__error">{error}</p>}
          <button type="submit" className="auth__submit" disabled={isSubmitting}>
            {isSubmitting ? 'Please wait…' : mode === 'login' ? 'Log in' : 'Create account'}
          </button>
        </form>
      </div>
    </div>
  );
}

export default Auth;
