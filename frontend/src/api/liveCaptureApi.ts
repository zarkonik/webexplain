import { apiClient, API_BASE_URL } from './client';
import type { LiveCaptureStepResponse, RecordedStepDto, StartLiveCaptureResponse } from '../types/liveCapture';

export function getLiveScreenshotUrl(sessionId: string, order: number): string {
  return `${API_BASE_URL}/api/live-capture/${sessionId}/screenshot/${order}`;
}

export async function startLiveCapture(url: string): Promise<StartLiveCaptureResponse> {
  const response = await apiClient.post<StartLiveCaptureResponse>('/api/live-capture/start', { url });
  return response.data;
}

export async function clickLiveCapture(
  sessionId: string,
  xRatio: number,
  yRatio: number,
): Promise<LiveCaptureStepResponse> {
  const response = await apiClient.post<LiveCaptureStepResponse>(`/api/live-capture/${sessionId}/click`, {
    xRatio,
    yRatio,
  });
  return response.data;
}

export async function finishLiveCapture(sessionId: string): Promise<RecordedStepDto[]> {
  const response = await apiClient.post<RecordedStepDto[]>(`/api/live-capture/${sessionId}/finish`);
  return response.data;
}
