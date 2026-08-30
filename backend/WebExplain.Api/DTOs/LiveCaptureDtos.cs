namespace WebExplain.Api.DTOs;

public record StartLiveCaptureRequest(string Url);

public record StartLiveCaptureResponse(Guid SessionId, int Order, string Url, int ViewportWidth, int ViewportHeight);

public record LiveCaptureClickRequest(double XRatio, double YRatio);

public record LiveCaptureStepResponse(int Order, string ActionType, string? Selector, string Url);

public record RecordedStepDto(int Order, string ActionType, string? Selector, string Url);
