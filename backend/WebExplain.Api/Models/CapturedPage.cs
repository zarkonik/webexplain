namespace WebExplain.Api.Models;

public class CapturedPage
{
    public Guid Id { get; set; }
    public Guid CaptureSessionId { get; set; }
    public CaptureSession? CaptureSession { get; set; }

    public int Order { get; set; }
    public string Url { get; set; } = string.Empty;
    public string HtmlFilePath { get; set; } = string.Empty;
    public string ScreenshotFilePath { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
}
