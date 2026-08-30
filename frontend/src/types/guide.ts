export interface CreateGuideStepRequest {
  order: number;
  targetSelector: string;
  instruction: string;
  actionType: string;
  inputValue?: string | null;
  pageUrl?: string | null;
  elementDescription?: string | null;
}

export interface CreateGuideRequest {
  title: string;
  description: string;
  sourceUrl: string;
  sourceCaptureSessionId?: string | null;
  steps: CreateGuideStepRequest[];
}

export interface GuideStepDto {
  id: string;
  order: number;
  targetSelector: string;
  instruction: string;
  actionType: string;
  inputValue: string | null;
  pageUrl: string | null;
  elementDescription: string | null;
}

export interface GuideDto {
  id: string;
  title: string;
  description: string;
  sourceUrl: string;
  sourceCaptureSessionId: string | null;
  createdAt: string;
  steps: GuideStepDto[];
}
