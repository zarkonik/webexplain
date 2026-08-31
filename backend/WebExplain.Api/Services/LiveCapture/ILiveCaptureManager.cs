using WebExplain.Api.DTOs;

namespace WebExplain.Api.Services.LiveCapture;

public interface ILiveCaptureManager
{
    Task<StartLiveCaptureResponse> StartAsync(string url, CancellationToken cancellationToken = default);
    Task<ElementProbe> InspectAsync(Guid sessionId, double xRatio, double yRatio, CancellationToken cancellationToken = default);
    Task<LiveCaptureStepResponse> ClickAsync(Guid sessionId, double xRatio, double yRatio, CancellationToken cancellationToken = default);
    Task<LiveCaptureStepResponse> FillAsync(Guid sessionId, double xRatio, double yRatio, string value, CancellationToken cancellationToken = default);
    Task<int> ScrollAsync(Guid sessionId, double deltaY, CancellationToken cancellationToken = default);
    string? GetScreenshotPath(Guid sessionId, int order);
    List<RecordedStep>? GetSteps(Guid sessionId);
    Task<List<RecordedStep>> FinishAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task ExpireIdleSessionsAsync(TimeSpan idleThreshold);
}
