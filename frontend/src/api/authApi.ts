import { apiClient } from './client';

export interface AuthResponse {
  token: string;
  email: string;
  expiresAt: string;
}

export async function register(email: string, password: string): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/api/auth/register', { email, password });
  return response.data;
}

export async function login(email: string, password: string): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/api/auth/login', { email, password });
  return response.data;
}
