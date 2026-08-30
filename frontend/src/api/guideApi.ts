import { apiClient } from './client';
import type { CreateGuideRequest, GuideDto } from '../types/guide';

export async function createGuide(request: CreateGuideRequest): Promise<GuideDto> {
  const response = await apiClient.post<GuideDto>('/api/guides', request);
  return response.data;
}

export async function getGuides(): Promise<GuideDto[]> {
  const response = await apiClient.get<GuideDto[]>('/api/guides');
  return response.data;
}

export async function deleteGuide(id: string): Promise<void> {
  await apiClient.delete(`/api/guides/${id}`);
}
