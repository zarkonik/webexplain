namespace WebExplain.Api.Services.Capture;

public record CaptureStepAction(string ActionType, string? Selector, string? Value);

public record PageCaptureResult(string Url, string HtmlFilePath, string ScreenshotFilePath);

public interface IBrowserCaptureEngine
{
    Task<List<PageCaptureResult>> CaptureAsync(
        string startUrl,
        IReadOnlyList<CaptureStepAction> steps,
        string outputFolder,
        CancellationToken cancellationToken = default);
}
