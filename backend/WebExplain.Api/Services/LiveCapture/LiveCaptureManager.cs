using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using WebExplain.Api.DTOs;
using WebExplain.Api.Models;
using WebExplain.Api.Repositories;

namespace WebExplain.Api.Services.LiveCapture;

public class LiveCaptureManager(IWebHostEnvironment environment, IServiceScopeFactory scopeFactory) : ILiveCaptureManager
{
    private readonly ConcurrentDictionary<Guid, LiveSession> _sessions = new();

    private const string MaskedValue = "••••••••";

    /// <summary>
    /// Chromium throttles timers, requestAnimationFrame and rendering on pages it treats as
    /// backgrounded/unfocused - which a headless, input-idle automation session always looks
    /// like unless something is actively clicking. That stalls JS-driven loading spinners and
    /// can make async work (that a spinner's own tick loop happens to drive) appear stuck.
    /// These flags disable that backgrounding behavior.
    /// </summary>
    private static readonly string[] ChromiumThrottlingWorkaroundArgs =
    [
        "--disable-background-timer-throttling",
        "--disable-backgrounding-occluded-windows",
        "--disable-renderer-backgrounding",
        "--disable-popup-blocking"
    ];

    private const string ProbeScript = """
        ([x, y]) => {
            const el = document.elementFromPoint(x, y);
            if (!el) return { selector: null, isFillable: false, isSensitive: false, tag: null, label: null, x: 0, y: 0, width: 0, height: 0 };

            const tag = el.tagName.toLowerCase();
            const type = (el.getAttribute('type') || '').toLowerCase();
            const nonTextInputTypes = ['button', 'submit', 'checkbox', 'radio', 'range', 'color', 'file', 'image', 'reset'];
            const isFillable = tag === 'textarea'
                || (tag === 'input' && !nonTextInputTypes.includes(type))
                || el.isContentEditable === true;
            const isSensitive = tag === 'input' && (type === 'password'
                || el.getAttribute('autocomplete') === 'current-password'
                || el.getAttribute('autocomplete') === 'new-password');

            function selectorFor(node) {
                if (node.id) return '#' + CSS.escape(node.id);
                const testId = node.getAttribute('data-testid');
                if (testId) return `[data-testid="${testId}"]`;
                const parts = [];
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

            function labelFor(node) {
                const aria = node.getAttribute('aria-label');
                if (aria && aria.trim()) return aria.trim();

                if (node.id) {
                    const labelEl = document.querySelector(`label[for="${CSS.escape(node.id)}"]`);
                    if (labelEl && labelEl.textContent && labelEl.textContent.trim()) {
                        return labelEl.textContent.trim();
                    }
                }

                const placeholder = node.getAttribute('placeholder');
                if (placeholder && placeholder.trim()) return placeholder.trim();

                const text = (node.innerText || node.textContent || '').trim().replace(/\s+/g, ' ');
                if (text) return text.slice(0, 60);

                return null;
            }

            const rect = el.getBoundingClientRect();
            return {
                selector: selectorFor(el),
                isFillable,
                isSensitive,
                tag,
                label: labelFor(el),
                x: rect.x,
                y: rect.y,
                width: rect.width,
                height: rect.height
            };
        }
        """;

    private const string StealthInitScript = """
        Object.defineProperty(navigator, 'webdriver', { get: () => undefined });
        Object.defineProperty(navigator, 'languages', { get: () => ['en-US', 'en'] });
        Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3, 4, 5] });

        if (!window.chrome) {
            window.chrome = { runtime: {} };
        }

        if (window.navigator.permissions && window.navigator.permissions.query) {
            const originalQuery = window.navigator.permissions.query.bind(window.navigator.permissions);
            window.navigator.permissions.query = (parameters) => (
                parameters.name === 'notifications'
                    ? Promise.resolve({ state: Notification.permission })
                    : originalQuery(parameters)
            );
        }
        """;

    public async Task<StartLiveCaptureResponse> StartAsync(string url, CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid();
        var storageFolder = Path.Combine(environment.ContentRootPath, "Storage", "Captures", sessionId.ToString());
        Directory.CreateDirectory(storageFolder);

        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = ChromiumThrottlingWorkaroundArgs
        });

        const int viewportWidth = 1280;
        const int viewportHeight = 800;

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = viewportWidth, Height = viewportHeight },
            RecordHarPath = Path.Combine(storageFolder, "network.har"),
            RecordHarContent = HarContentPolicy.Embed,
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36"
        });

        // Payment/fraud-detection flows (PayPal sandbox included) can detect the tell-tale
        // signs of an automated/headless browser and deliberately stall instead of failing
        // outright - which looks exactly like a stuck "in progress" spinner from the outside.
        // This patches the most common automation fingerprints before any page script runs.
        await context.AddInitScriptAsync(StealthInitScript);

        var page = await context.NewPageAsync();
        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await SettlePageAsync(page);

        var session = new LiveSession(playwright, browser, context, page, url, storageFolder, viewportWidth, viewportHeight);
        var initialStep = await CaptureStepAsync(session, "navigate", null, null, "Open the starting page.", cancellationToken);
        _sessions[sessionId] = session;

        return new StartLiveCaptureResponse(sessionId, initialStep.Order, initialStep.Url, viewportWidth, viewportHeight);
    }

    public async Task<ElementProbe> InspectAsync(Guid sessionId, double xRatio, double yRatio, CancellationToken cancellationToken = default)
    {
        var session = GetActiveSession(sessionId);
        var (viewportWidth, viewportHeight) = GetActiveViewportSize(session);
        var x = xRatio * viewportWidth;
        var y = yRatio * viewportHeight;

        return await session.ActivePage.EvaluateAsync<ElementProbe>(ProbeScript, new[] { x, y });
    }

    public async Task<LiveCaptureStepResponse> ClickAsync(Guid sessionId, double xRatio, double yRatio, CancellationToken cancellationToken = default)
    {
        var session = GetActiveSession(sessionId);
        session.LastActivity = DateTime.UtcNow;

        var activePage = session.ActivePage;
        var (viewportWidth, viewportHeight) = GetActiveViewportSize(session);
        var x = xRatio * viewportWidth;
        var y = yRatio * viewportHeight;

        var probe = await activePage.EvaluateAsync<ElementProbe>(ProbeScript, new[] { x, y });

        await ClickAndAttachAnyPopupAsync(session, activePage, x, y);
        await SettlePageAsync(session.ActivePage);

        var description = BuildElementDescription("click", probe, null);
        var step = await CaptureStepAsync(session, "click", probe.Selector, null, description, cancellationToken, probe);
        return new LiveCaptureStepResponse(
            step.Order, step.ActionType, step.Selector, step.Value, step.ElementDescription, step.Url,
            step.TargetX, step.TargetY, step.TargetWidth, step.TargetHeight,
            IsPopup: session.Popup is { IsClosed: false });
    }

    public async Task<LiveCaptureStepResponse> FillAsync(
        Guid sessionId, double xRatio, double yRatio, string value, CancellationToken cancellationToken = default)
    {
        var session = GetActiveSession(sessionId);
        session.LastActivity = DateTime.UtcNow;

        var activePage = session.ActivePage;
        var (viewportWidth, viewportHeight) = GetActiveViewportSize(session);
        var x = xRatio * viewportWidth;
        var y = yRatio * viewportHeight;

        var probe = await activePage.EvaluateAsync<ElementProbe>(ProbeScript, new[] { x, y });

        await activePage.Mouse.ClickAsync((float)x, (float)y);
        await activePage.Keyboard.PressAsync("Control+A");
        await activePage.Keyboard.PressAsync("Delete");
        await activePage.Keyboard.TypeAsync(value);
        await SettlePageAsync(session.ActivePage);

        // The real value is only ever used to type into the page above - everything
        // recorded, returned, or persisted from here on uses the masked form for
        // sensitive fields (passwords, etc.) so plaintext secrets never leave this method.
        // The raw value is kept briefly in memory so FinishAsync can scrub it from the HAR.
        if (probe.IsSensitive)
        {
            session.SensitiveValues.Add(value);
        }
        var recordedValue = probe.IsSensitive ? MaskedValue : value;

        var description = BuildElementDescription("fill", probe, recordedValue);
        var step = await CaptureStepAsync(session, "fill", probe.Selector, recordedValue, description, cancellationToken, probe);
        return new LiveCaptureStepResponse(
            step.Order, step.ActionType, step.Selector, step.Value, step.ElementDescription, step.Url,
            step.TargetX, step.TargetY, step.TargetWidth, step.TargetHeight,
            IsPopup: session.Popup is { IsClosed: false });
    }

    public async Task<int> ScrollAsync(Guid sessionId, double deltaY, CancellationToken cancellationToken = default)
    {
        var session = GetActiveSession(sessionId);
        session.LastActivity = DateTime.UtcNow;

        var activePage = session.ActivePage;
        await activePage.Mouse.WheelAsync(0, (float)deltaY);
        await SettlePageAsync(activePage);

        // Scrolling isn't a recordable action on its own - it just changes what the
        // current step looks like, so the latest step's screenshot/HTML are refreshed
        // in place rather than appending a new step.
        var latestStep = session.Steps.LastOrDefault();
        if (latestStep is null)
        {
            return 0;
        }

        var html = await WithNavigationRetryAsync(() => activePage.ContentAsync());
        await File.WriteAllTextAsync(latestStep.HtmlFilePath, html, cancellationToken);
        await WithNavigationRetryAsync(async () =>
        {
            await activePage.ScreenshotAsync(new PageScreenshotOptions { Path = latestStep.ScreenshotFilePath });
            return true;
        });

        return latestStep.Order;
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

        // The HAR is only flushed to disk once the context above closes, so redaction
        // has to happen here rather than at the moment each sensitive value was typed.
        await RedactHarFileAsync(session);

        await PersistSessionAsync(sessionId, session);

        return session.Steps;
    }

    private const string RedactedPlaceholder = "[REDACTED]";

    private static async Task RedactHarFileAsync(LiveSession session)
    {
        if (session.SensitiveValues.Count == 0)
        {
            return;
        }

        var harPath = Path.Combine(session.StorageFolder, "network.har");
        if (!File.Exists(harPath))
        {
            session.SensitiveValues.Clear();
            return;
        }

        var content = await File.ReadAllTextAsync(harPath);

        foreach (var secret in session.SensitiveValues.Distinct())
        {
            if (string.IsNullOrEmpty(secret))
            {
                continue;
            }

            content = content.Replace(secret, RedactedPlaceholder);

            // Login forms commonly submit as application/x-www-form-urlencoded, where the
            // value is percent-encoded (and spaces become '+') rather than appearing verbatim.
            var percentEncoded = Uri.EscapeDataString(secret);
            content = content.Replace(percentEncoded, RedactedPlaceholder);
            content = content.Replace(percentEncoded.Replace("%20", "+"), RedactedPlaceholder);
        }

        await File.WriteAllTextAsync(harPath, content);
        session.SensitiveValues.Clear();
    }

    private async Task PersistSessionAsync(Guid sessionId, LiveSession session)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICaptureSessionRepository>();

        await repository.AddAsync(new CaptureSession
        {
            Id = sessionId,
            SourceUrl = session.StartUrl,
            Status = CaptureStatus.Completed,
            StorageFolder = session.StorageFolder,
            CompletedAt = DateTime.UtcNow
        });

        foreach (var step in session.Steps)
        {
            await repository.AddPageAsync(new CapturedPage
            {
                Id = Guid.NewGuid(),
                CaptureSessionId = sessionId,
                Order = step.Order,
                Url = step.Url,
                HtmlFilePath = step.HtmlFilePath,
                ScreenshotFilePath = step.ScreenshotFilePath
            });
        }
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

    private static string BuildElementDescription(string actionType, ElementProbe probe, string? value)
    {
        var friendlyTag = probe.Tag switch
        {
            "button" => "button",
            "a" => "link",
            "input" => "field",
            "textarea" => "field",
            "select" => "dropdown",
            "img" => "image",
            _ => "element"
        };

        var label = string.IsNullOrWhiteSpace(probe.Label) ? null : probe.Label;

        if (actionType == "fill")
        {
            var valuePhrase = probe.IsSensitive ? "a hidden value" : $"\"{value}\"";
            return label is not null
                ? $"Type {valuePhrase} into the \"{label}\" {friendlyTag}."
                : $"Type {valuePhrase} into the highlighted {friendlyTag}.";
        }

        return label is not null
            ? $"Click the \"{label}\" {friendlyTag}."
            : $"Click the highlighted {friendlyTag}.";
    }

    /// <summary>
    /// Some flows (payment/OAuth-style "test mode" dialogs in particular) open a popup window
    /// that needs its own clicks/typing to complete - waiting for it to close on its own never
    /// finishes if nothing ever interacts with it. This performs the click and, if a popup
    /// opens, attaches it to the session as the new <see cref="LiveSession.ActivePage"/>: every
    /// screenshot, click, fill and scroll from this point targets the popup instead of the main
    /// page, until the popup closes itself, at which point control reverts to the main page
    /// automatically (see <see cref="LiveSession.ActivePage"/>).
    /// </summary>
    private static async Task ClickAndAttachAnyPopupAsync(LiveSession session, IPage page, double x, double y)
    {
        IPage? popup = null;
        void OnPopup(object? sender, IPage p) => popup = p;

        session.Context.Page += OnPopup;
        try
        {
            await page.Mouse.ClickAsync((float)x, (float)y);
            // Give a popup, if one is coming, time to actually open - some apps do async work
            // (e.g. fetching a payment session) before calling window.open(), so this needs to
            // be generous rather than assuming the popup appears immediately after the click.
            await Task.Delay(3000);
        }
        finally
        {
            session.Context.Page -= OnPopup;
        }

        if (popup is null)
        {
            return;
        }

        session.Popup = popup;
        popup.Close += (_, _) =>
        {
            if (ReferenceEquals(session.Popup, popup))
            {
                session.Popup = null;
            }
        };

        try
        {
            await popup.WaitForLoadStateAsync(LoadState.Load, new PageWaitForLoadStateOptions { Timeout = 5000 });
        }
        catch (TimeoutException)
        {
            // The popup may still be blank/loading - it'll show up as-is in the screenshot.
        }
    }

    /// <summary>
    /// Popups aren't guaranteed to open at the fixed 1280x800 the main page uses - sites often
    /// size them explicitly (e.g. a small PayPal dialog). Click/fill coordinates are sent as
    /// ratios from the frontend specifically so they can be scaled against whichever page -
    /// main or popup - is actually active right now.
    /// </summary>
    private static (int Width, int Height) GetActiveViewportSize(LiveSession session)
    {
        var viewport = session.ActivePage.ViewportSize;
        return viewport is not null ? (viewport.Width, viewport.Height) : (session.ViewportWidth, session.ViewportHeight);
    }

    /// <summary>
    /// Waits for network idle, re-checking up to <paramref name="maxAttempts"/> times before
    /// giving up - client-side apps often fire several rounds of sequential requests (e.g. a
    /// multi-step payment flow) and a single network-idle wait can capture the screenshot
    /// mid-transaction. Stops early once idle is reached twice in a row, so a simple click on
    /// an already-settled page doesn't pay the full retry budget.
    /// </summary>
    private static async Task SettlePageAsync(IPage page, int maxAttempts = 5)
    {
        var consecutiveIdles = 0;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Keeps the page's Page Visibility API state as "visible" so JS-driven spinners
            // and polling loops that check document.visibilityState don't stall on their own.
            await page.BringToFrontAsync();

            try
            {
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 3000 });
                consecutiveIdles++;
            }
            catch (TimeoutException)
            {
                // Still busy (e.g. a transaction in progress) - keep retrying.
                consecutiveIdles = 0;
            }

            if (consecutiveIdles >= 2)
            {
                return;
            }

            await Task.Delay(1500);
        }
    }

    private static async Task<RecordedStep> CaptureStepAsync(
        LiveSession session,
        string actionType,
        string? selector,
        string? value,
        string? elementDescription,
        CancellationToken cancellationToken,
        ElementProbe? targetProbe = null)
    {
        var order = session.Steps.Count + 1;
        var htmlFilePath = Path.Combine(session.StorageFolder, $"page-{order}.html");
        var screenshotFilePath = Path.Combine(session.StorageFolder, $"screenshot-{order}.png");
        var activePage = session.ActivePage;

        var html = await WithNavigationRetryAsync(() => activePage.ContentAsync());
        await File.WriteAllTextAsync(htmlFilePath, html, cancellationToken);
        await WithNavigationRetryAsync(async () =>
        {
            await activePage.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotFilePath });
            return true;
        });

        var step = new RecordedStep(
            order, actionType, selector, value, elementDescription, activePage.Url, htmlFilePath, screenshotFilePath,
            targetProbe?.X, targetProbe?.Y, targetProbe?.Width, targetProbe?.Height);
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
