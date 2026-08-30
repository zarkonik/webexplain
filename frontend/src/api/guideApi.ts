import { apiClient } from './client';
import type { CreateGuideRequest, GuideDto } from '../types/guide';

export async function createGuide(request: CreateGuideRequest): Promise<GuideDto> {
  const response = await apiClient.post<GuideDto>('/api/guides', request);
  return response.data;
}
