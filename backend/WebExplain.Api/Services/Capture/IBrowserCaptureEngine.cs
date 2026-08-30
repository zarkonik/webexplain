namespace WebExplain.Api.Services.Capture;

public record PageCaptureResult(string Url, string HtmlFilePath, string ScreenshotFilePath, string HarFilePath);

public interface IBrowserCaptureEngine
{
    Task<PageCaptureResult> CaptureAsync(string url, string outputFolder, CancellationToken cancellationToken = default);
}
