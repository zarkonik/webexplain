export interface StartLiveCaptureResponse {
  sessionId: string;
  order: number;
  url: string;
  viewportWidth: number;
  viewportHeight: number;
}

export interface LiveCaptureInspectResponse {
  selector: string | null;
  isFillable: boolean;
}

export interface LiveCaptureStepResponse {
  order: number;
  actionType: string;
  selector: string | null;
  value: string | null;
  elementDescription: string | null;
  url: string;
  targetX: number | null;
  targetY: number | null;
  targetWidth: number | null;
  targetHeight: number | null;
}

export interface RecordedStepDto {
  order: number;
  actionType: string;
  selector: string | null;
  value: string | null;
  elementDescription: string | null;
  url: string;
  targetX: number | null;
  targetY: number | null;
  targetWidth: number | null;
  targetHeight: number | null;
}
