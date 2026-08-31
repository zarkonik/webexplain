namespace WebExplain.Api.DTOs;

public record GuideStepDto(
    Guid Id,
    int Order,
    string TargetSelector,
    string Instruction,
    string ActionType,
    string? InputValue,
    string? PageUrl,
    string? ElementDescription,
    double? TargetX,
    double? TargetY,
    double? TargetWidth,
    double? TargetHeight
);

public record GuideDto(
    Guid Id,
    string Title,
    string Description,
    string SourceUrl,
    Guid? SourceCaptureSessionId,
    DateTime CreatedAt,
    List<GuideStepDto> Steps
);

public record CreateGuideStepRequest(
    int Order,
    string TargetSelector,
    string Instruction,
    string ActionType,
    string? InputValue,
    string? PageUrl,
    string? ElementDescription,
    double? TargetX = null,
    double? TargetY = null,
    double? TargetWidth = null,
    double? TargetHeight = null
);

public record CreateGuideRequest(
    string Title,
    string Description,
    string SourceUrl,
    Guid? SourceCaptureSessionId,
    List<CreateGuideStepRequest> Steps
);
