using WebExplain.Api.DTOs;

namespace WebExplain.Api.Services;

public interface IGuideService
{
    Task<List<GuideDto>> GetAllGuidesAsync(Guid userId);
    Task<GuideDto?> GetGuideByIdAsync(Guid id, Guid userId);
    Task<GuideDto> CreateGuideAsync(CreateGuideRequest request, Guid userId);
    Task<bool> DeleteGuideAsync(Guid id, Guid userId);
}
