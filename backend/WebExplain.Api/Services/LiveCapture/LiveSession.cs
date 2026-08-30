using Microsoft.Playwright;

namespace WebExplain.Api.Services.LiveCapture;

public record RecordedStep(
    int Order,
    string ActionType,
    string? Selector,
    string? Value,
    string? ElementDescription,
    string Url,
    string HtmlFilePath,
    string ScreenshotFilePath);

public class ElementProbe
{
    public string? Selector { get; set; }
    public bool IsFillable { get; set; }
    public bool IsSensitive { get; set; }
    public string? Tag { get; set; }
    public string? Label { get; set; }
}

public class LiveSession(
    IPlaywright playwright,
    IBrowser browser,
    IBrowserContext context,
    IPage page,
    string startUrl,
    string storageFolder,
    int viewportWidth,
    int viewportHeight)
{
    public IPlaywright Playwright { get; } = playwright;
    public IBrowser Browser { get; } = browser;
    public IBrowserContext Context { get; } = context;
    public IPage Page { get; } = page;
    public string StartUrl { get; } = startUrl;
    public string StorageFolder { get; } = storageFolder;
    public int ViewportWidth { get; } = viewportWidth;
    public int ViewportHeight { get; } = viewportHeight;
    public List<RecordedStep> Steps { get; } = [];
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public bool IsFinished { get; set; }

    /// <summary>
    /// Real (unmasked) values typed into sensitive fields, held only long enough to redact
    /// them from the recorded HAR file once the session ends. Never persisted or returned
    /// to the client - cleared immediately after redaction.
    /// </summary>
    public List<string> SensitiveValues { get; } = [];
}
