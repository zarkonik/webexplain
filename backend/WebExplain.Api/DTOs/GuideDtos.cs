namespace WebExplain.Api.DTOs;

public record GuideStepDto(
    Guid Id,
    int Order,
    string TargetSelector,
    string Instruction,
    string ActionType,
    string? InputValue
);

public record GuideDto(
    Guid Id,
    string Title,
    string Description,
    string SourceUrl,
    DateTime CreatedAt,
    List<GuideStepDto> Steps
);

public record CreateGuideStepRequest(
    int Order,
    string TargetSelector,
    string Instruction,
    string ActionType,
    string? InputValue
);

public record CreateGuideRequest(
    string Title,
    string Description,
    string SourceUrl,
    List<CreateGuideStepRequest> Steps
);
