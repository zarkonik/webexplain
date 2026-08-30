export interface StartLiveCaptureResponse {
  sessionId: string;
  order: number;
  url: string;
  viewportWidth: number;
  viewportHeight: number;
}

export interface LiveCaptureStepResponse {
  order: number;
  actionType: string;
  selector: string | null;
  url: string;
}

export interface RecordedStepDto {
  order: number;
  actionType: string;
  selector: string | null;
  url: string;
}
