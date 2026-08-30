export const CaptureStatus = {
  Pending: 0,
  Running: 1,
  Completed: 2,
  Failed: 3,
} as const;

export type CaptureStatus = (typeof CaptureStatus)[keyof typeof CaptureStatus];

export type CaptureActionType = 'click' | 'fill' | 'navigate';

export interface CaptureStepInput {
  actionType: CaptureActionType;
  selector?: string;
  value?: string;
}

export interface CapturedPageDto {
  id: string;
  order: number;
  url: string;
  htmlFilePath: string;
  screenshotFilePath: string;
  capturedAt: string;
}

export interface CaptureSessionDto {
  id: string;
  sourceUrl: string;
  status: CaptureStatus;
  errorMessage: string | null;
  createdAt: string;
  completedAt: string | null;
  pages: CapturedPageDto[];
}
