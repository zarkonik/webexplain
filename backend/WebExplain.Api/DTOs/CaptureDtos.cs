using WebExplain.Api.Models;

namespace WebExplain.Api.DTOs;

public record CaptureActionRequest(string ActionType, string? Selector, string? Value);

public record CreateCaptureRequest(string Url, List<CaptureActionRequest>? Steps = null);

public record CapturedPageDto(
    Guid Id,
    int Order,
    string Url,
    string HtmlFilePath,
    string ScreenshotFilePath,
    DateTime CapturedAt
);

public record CaptureSessionDto(
    Guid Id,
    string SourceUrl,
    CaptureStatus Status,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    List<CapturedPageDto> Pages
);
