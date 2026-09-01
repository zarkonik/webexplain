using WebExplain.Api.DTOs;
using WebExplain.Api.Models;
using WebExplain.Api.Repositories;

namespace WebExplain.Api.Services;

public class GuideService(IGuideRepository repository) : IGuideService
{
    public async Task<List<GuideDto>> GetAllGuidesAsync(Guid userId)
    {
        var guides = await repository.GetAllAsync(userId);
        return guides.Select(ToDto).ToList();
    }

    public async Task<GuideDto?> GetGuideByIdAsync(Guid id, Guid userId)
    {
        var guide = await repository.GetByIdAsync(id, userId);
        return guide is null ? null : ToDto(guide);
    }

    public async Task<GuideDto> CreateGuideAsync(CreateGuideRequest request, Guid userId)
    {
        var guide = new Guide
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title,
            Description = request.Description,
            SourceUrl = request.SourceUrl,
            SourceCaptureSessionId = request.SourceCaptureSessionId,
            Steps = request.Steps.Select(s => new GuideStep
            {
                Id = Guid.NewGuid(),
                Order = s.Order,
                TargetSelector = s.TargetSelector,
                Instruction = s.Instruction,
                ActionType = s.ActionType,
                InputValue = s.InputValue,
                PageUrl = s.PageUrl,
                ElementDescription = s.ElementDescription,
                TargetX = s.TargetX,
                TargetY = s.TargetY,
                TargetWidth = s.TargetWidth,
                TargetHeight = s.TargetHeight
            }).ToList()
        };

        var created = await repository.AddAsync(guide);
        return ToDto(created);
    }

    public Task<bool> DeleteGuideAsync(Guid id, Guid userId) => repository.DeleteAsync(id, userId);

    private static GuideDto ToDto(Guide guide) => new(
        guide.Id,
        guide.Title,
        guide.Description,
        guide.SourceUrl,
        guide.SourceCaptureSessionId,
        guide.CreatedAt,
        guide.Steps.Select(s => new GuideStepDto(
            s.Id, s.Order, s.TargetSelector, s.Instruction, s.ActionType, s.InputValue, s.PageUrl, s.ElementDescription,
            s.TargetX, s.TargetY, s.TargetWidth, s.TargetHeight
        )).ToList()
    );
}
