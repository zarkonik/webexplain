import { apiClient, API_BASE_URL } from './client';
import type { CaptureSessionDto, CaptureStepInput } from '../types/capture';

export function getScreenshotUrl(sessionId: string, order: number): string {
  return `${API_BASE_URL}/api/capture/${sessionId}/screenshot/${order}`;
}

export async function getCaptureSessions(): Promise<CaptureSessionDto[]> {
  const response = await apiClient.get<CaptureSessionDto[]>('/api/capture');
  return response.data;
}

export async function createCaptureSession(
  url: string,
  steps: CaptureStepInput[] = [],
): Promise<CaptureSessionDto> {
  const response = await apiClient.post<CaptureSessionDto>('/api/capture', { url, steps });
  return response.data;
}
