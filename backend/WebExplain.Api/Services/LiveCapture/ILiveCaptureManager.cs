using WebExplain.Api.DTOs;

namespace WebExplain.Api.Services.LiveCapture;

public interface ILiveCaptureManager
{
    Task<StartLiveCaptureResponse> StartAsync(string url, Guid userId, CancellationToken cancellationToken = default);
    Task<ElementProbe> InspectAsync(Guid sessionId, Guid userId, double xRatio, double yRatio, CancellationToken cancellationToken = default);
    Task<LiveCaptureStepResponse> ClickAsync(Guid sessionId, Guid userId, double xRatio, double yRatio, CancellationToken cancellationToken = default);
    Task<LiveCaptureStepResponse> FillAsync(Guid sessionId, Guid userId, double xRatio, double yRatio, string value, CancellationToken cancellationToken = default);
    Task<int> ScrollAsync(Guid sessionId, Guid userId, double deltaY, CancellationToken cancellationToken = default);
    string? GetScreenshotPath(Guid sessionId, Guid userId, int order);
    List<RecordedStep>? GetSteps(Guid sessionId, Guid userId);
    Task<List<RecordedStep>> FinishAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default);
    Task ExpireIdleSessionsAsync(TimeSpan idleThreshold);
}
