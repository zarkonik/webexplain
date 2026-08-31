using Microsoft.Playwright;

namespace WebExplain.Api.Services.Capture;

public class PlaywrightCaptureEngine : IBrowserCaptureEngine
{
    public async Task<List<PageCaptureResult>> CaptureAsync(
        string startUrl,
        IReadOnlyList<CaptureStepAction> steps,
        string outputFolder,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(outputFolder);
        var harFilePath = Path.Combine(outputFolder, "network.har");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args =
            [
                "--disable-background-timer-throttling",
                "--disable-backgrounding-occluded-windows",
                "--disable-renderer-backgrounding"
            ]
        });

        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            RecordHarPath = harFilePath,
            RecordHarContent = HarContentPolicy.Embed
        });

        var page = await context.NewPageAsync();
        var results = new List<PageCaptureResult>();

        await page.GotoAsync(startUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        results.Add(await CapturePageStateAsync(page, outputFolder, results.Count + 1, cancellationToken));

        foreach (var step in steps)
        {
            await ExecuteStepAsync(page, step);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            results.Add(await CapturePageStateAsync(page, outputFolder, results.Count + 1, cancellationToken));
        }

        // Closing the context flushes the recorded HAR file to disk.
        await context.CloseAsync();

        return results;
    }

    private static Task ExecuteStepAsync(IPage page, CaptureStepAction step)
    {
        return step.ActionType.ToLowerInvariant() switch
        {
            "click" => page.ClickAsync(RequireSelector(step)),
            "fill" => page.FillAsync(RequireSelector(step), step.Value ?? string.Empty),
            "navigate" => page.GotoAsync(RequireValue(step), new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle }),
            _ => throw new NotSupportedException($"Unsupported capture action type: {step.ActionType}")
        };
    }

    private static string RequireSelector(CaptureStepAction step) =>
        step.Selector ?? throw new ArgumentException($"Action '{step.ActionType}' requires a selector.");

    private static string RequireValue(CaptureStepAction step) =>
        step.Value ?? throw new ArgumentException($"Action '{step.ActionType}' requires a value.");

    private static async Task<PageCaptureResult> CapturePageStateAsync(
        IPage page, string outputFolder, int order, CancellationToken cancellationToken)
    {
        var htmlFilePath = Path.Combine(outputFolder, $"page-{order}.html");
        var screenshotFilePath = Path.Combine(outputFolder, $"screenshot-{order}.png");

        var html = await page.ContentAsync();
        await File.WriteAllTextAsync(htmlFilePath, html, cancellationToken);
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotFilePath, FullPage = true });

        return new PageCaptureResult(page.Url, htmlFilePath, screenshotFilePath);
    }
}
