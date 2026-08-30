export enum CaptureStatus {
  Pending = 0,
  Running = 1,
  Completed = 2,
  Failed = 3,
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
