using System.Collections.Concurrent;
using Microsoft.Playwright;
using WebExplain.Api.DTOs;

namespace WebExplain.Api.Services.LiveCapture;

public class LiveCaptureManager(IWebHostEnvironment environment) : ILiveCaptureManager
{
    private readonly ConcurrentDictionary<Guid, LiveSession> _sessions = new();

    private const string SelectorScript = """
        ([x, y]) => {
            const el = document.elementFromPoint(x, y);
            if (!el) return null;
            if (el.id) return '#' + CSS.escape(el.id);
            const testId = el.getAttribute('data-testid');
            if (testId) return `[data-testid="${testId}"]`;
            const parts = [];
            let node = el;
            let depth = 0;
            while (node && node.nodeType === 1 && depth < 4) {
                let sel = node.tagName.toLowerCase();
                const parent = node.parentElement;
                if (parent) {
                    const siblings = Array.from(parent.children).filter(c => c.tagName === node.tagName);
                    if (siblings.length > 1) {
                        sel += ':nth-of-type(' + (siblings.indexOf(node) + 1) + ')';
                    }
                }
                parts.unshift(sel);
                node = parent;
                depth++;
            }
            return parts.join(' > ');
        }
        """;

    public async Task<StartLiveCaptureResponse> StartAsync(string url, CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid();
        var storageFolder = Path.Combine(environment.ContentRootPath, "Storage", "Captures", sessionId.ToString());
        Directory.CreateDirectory(storageFolder);

        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

        const int viewportWidth = 1280;
        const int viewportHeight = 800;

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = viewportWidth, Height = viewportHeight },
            RecordHarPath = Path.Combine(storageFolder, "network.har"),
            RecordHarContent = HarContentPolicy.Embed
        });

        var page = await context.NewPageAsync();
        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var session = new LiveSession(playwright, browser, context, page, url, storageFolder, viewportWidth, viewportHeight);
        var initialStep = await CaptureStepAsync(session, "navigate", null, cancellationToken);
        _sessions[sessionId] = session;

        return new StartLiveCaptureResponse(sessionId, initialStep.Order, initialStep.Url, viewportWidth, viewportHeight);
    }

    public async Task<LiveCaptureStepResponse> ClickAsync(Guid sessionId, double xRatio, double yRatio, CancellationToken cancellationToken = default)
    {
        var session = GetActiveSession(sessionId);
        session.LastActivity = DateTime.UtcNow;

        var x = xRatio * session.ViewportWidth;
        var y = yRatio * session.ViewportHeight;

        var selector = await session.Page.EvaluateAsync<string?>(SelectorScript, new[] { x, y });

        await session.Page.Mouse.ClickAsync((float)x, (float)y);
        try
        {
            await session.Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 3000 });
        }
        catch (TimeoutException)
        {
            // Not every click triggers navigation or network activity - that's fine.
        }

        var step = await CaptureStepAsync(session, "click", selector, cancellationToken);
        return new LiveCaptureStepResponse(step.Order, step.ActionType, step.Selector, step.Url);
    }

    public string? GetScreenshotPath(Guid sessionId, int order)
    {
        return _sessions.TryGetValue(sessionId, out var session)
            ? session.Steps.FirstOrDefault(s => s.Order == order)?.ScreenshotFilePath
            : null;
    }

    public List<RecordedStep>? GetSteps(Guid sessionId)
    {
        return _sessions.TryGetValue(sessionId, out var session) ? session.Steps : null;
    }

    public async Task<List<RecordedStep>> FinishAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = GetActiveSession(sessionId);

        await session.Context.CloseAsync();
        await session.Browser.CloseAsync();
        session.Playwright.Dispose();
        session.IsFinished = true;
        session.LastActivity = DateTime.UtcNow;

        return session.Steps;
    }

    public async Task ExpireIdleSessionsAsync(TimeSpan idleThreshold)
    {
        var cutoff = DateTime.UtcNow - idleThreshold;
        var staleIds = _sessions
            .Where(kvp => !kvp.Value.IsFinished && kvp.Value.LastActivity < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var id in staleIds)
        {
            try
            {
                await FinishAsync(id);
            }
            catch
            {
                // Best-effort cleanup; an already-broken session shouldn't stop the sweep.
            }
        }

        var evictCutoff = DateTime.UtcNow - TimeSpan.FromHours(1);
        var evictIds = _sessions
            .Where(kvp => kvp.Value.IsFinished && kvp.Value.LastActivity < evictCutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var id in evictIds)
        {
            _sessions.TryRemove(id, out _);
        }
    }

    private LiveSession GetActiveSession(Guid sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            throw new InvalidOperationException("Live capture session not found or expired.");

        if (session.IsFinished)
            throw new InvalidOperationException("Live capture session has already finished.");

        return session;
    }

    private static async Task<RecordedStep> CaptureStepAsync(
        LiveSession session, string actionType, string? selector, CancellationToken cancellationToken)
    {
        var order = session.Steps.Count + 1;
        var htmlFilePath = Path.Combine(session.StorageFolder, $"page-{order}.html");
        var screenshotFilePath = Path.Combine(session.StorageFolder, $"screenshot-{order}.png");

        var html = await WithNavigationRetryAsync(() => session.Page.ContentAsync());
        await File.WriteAllTextAsync(htmlFilePath, html, cancellationToken);
        await WithNavigationRetryAsync(async () =>
        {
            await session.Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotFilePath });
            return true;
        });

        var step = new RecordedStep(order, actionType, selector, session.Page.Url, htmlFilePath, screenshotFilePath);
        session.Steps.Add(step);
        return step;
    }

    private static async Task<T> WithNavigationRetryAsync<T>(Func<Task<T>> action)
    {
        const int maxAttempts = 5;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await action();
            }
            catch (PlaywrightException) when (attempt < maxAttempts)
            {
                await Task.Delay(300);
            }
        }
    }
}
