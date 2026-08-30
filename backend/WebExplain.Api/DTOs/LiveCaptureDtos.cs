namespace WebExplain.Api.DTOs;

public record StartLiveCaptureRequest(string Url);

public record StartLiveCaptureResponse(Guid SessionId, int Order, string Url, int ViewportWidth, int ViewportHeight);

public record LiveCaptureClickRequest(double XRatio, double YRatio);

public record LiveCaptureInspectRequest(double XRatio, double YRatio);

public record LiveCaptureInspectResponse(string? Selector, bool IsFillable);

public record LiveCaptureFillRequest(double XRatio, double YRatio, string Value);

public record LiveCaptureStepResponse(int Order, string ActionType, string? Selector, string? Value, string? ElementDescription, string Url);

public record RecordedStepDto(int Order, string ActionType, string? Selector, string? Value, string? ElementDescription, string Url);
