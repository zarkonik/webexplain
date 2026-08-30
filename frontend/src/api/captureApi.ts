import { apiClient, API_BASE_URL } from './client';
import type { CaptureSessionDto } from '../types/capture';

export function getScreenshotUrl(sessionId: string): string {
  return `${API_BASE_URL}/api/capture/${sessionId}/screenshot`;
}

export async function getCaptureSessions(): Promise<CaptureSessionDto[]> {
  const response = await apiClient.get<CaptureSessionDto[]>('/api/capture');
  return response.data;
}

export async function createCaptureSession(url: string): Promise<CaptureSessionDto> {
  const response = await apiClient.post<CaptureSessionDto>('/api/capture', { url });
  return response.data;
}
