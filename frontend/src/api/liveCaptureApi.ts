import { apiClient, API_BASE_URL } from './client';
import type {
  LiveCaptureInspectResponse,
  LiveCaptureStepResponse,
  RecordedStepDto,
  StartLiveCaptureResponse,
} from '../types/liveCapture';

export function getLiveScreenshotUrl(sessionId: string, order: number, version?: number): string {
  const base = `${API_BASE_URL}/api/live-capture/${sessionId}/screenshot/${order}`;
  return version ? `${base}?v=${version}` : base;
}

export async function startLiveCapture(url: string): Promise<StartLiveCaptureResponse> {
  const response = await apiClient.post<StartLiveCaptureResponse>('/api/live-capture/start', { url });
  return response.data;
}

export async function inspectLiveCapture(
  sessionId: string,
  xRatio: number,
  yRatio: number,
): Promise<LiveCaptureInspectResponse> {
  const response = await apiClient.post<LiveCaptureInspectResponse>(`/api/live-capture/${sessionId}/inspect`, {
    xRatio,
    yRatio,
  });
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

export async function fillLiveCapture(
  sessionId: string,
  xRatio: number,
  yRatio: number,
  value: string,
): Promise<LiveCaptureStepResponse> {
  const response = await apiClient.post<LiveCaptureStepResponse>(`/api/live-capture/${sessionId}/fill`, {
    xRatio,
    yRatio,
    value,
  });
  return response.data;
}

export async function scrollLiveCapture(sessionId: string, deltaY: number): Promise<number> {
  const response = await apiClient.post<{ order: number }>(`/api/live-capture/${sessionId}/scroll`, { deltaY });
  return response.data.order;
}

export async function finishLiveCapture(sessionId: string): Promise<RecordedStepDto[]> {
  const response = await apiClient.post<RecordedStepDto[]>(`/api/live-capture/${sessionId}/finish`);
  return response.data;
}
