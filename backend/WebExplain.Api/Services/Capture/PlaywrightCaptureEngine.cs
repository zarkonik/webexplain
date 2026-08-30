using Microsoft.Playwright;

namespace WebExplain.Api.Services.Capture;

public class PlaywrightCaptureEngine : IBrowserCaptureEngine
{
    public async Task<PageCaptureResult> CaptureAsync(string url, string outputFolder, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(outputFolder);
        var harFilePath = Path.Combine(outputFolder, "network.har");
        var htmlFilePath = Path.Combine(outputFolder, "page.html");
        var screenshotFilePath = Path.Combine(outputFolder, "screenshot.png");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            RecordHarPath = harFilePath,
            RecordHarContent = HarContentPolicy.Embed
        });

        var page = await context.NewPageAsync();
        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var html = await page.ContentAsync();
        await File.WriteAllTextAsync(htmlFilePath, html, cancellationToken);

        await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotFilePath, FullPage = true });

        // Closing the context flushes the recorded HAR file to disk.
        await context.CloseAsync();

        return new PageCaptureResult(url, htmlFilePath, screenshotFilePath, harFilePath);
    }
}
