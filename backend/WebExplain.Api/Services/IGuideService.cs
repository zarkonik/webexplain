using WebExplain.Api.DTOs;

namespace WebExplain.Api.Services;

public interface IGuideService
{
    Task<List<GuideDto>> GetAllGuidesAsync();
    Task<GuideDto?> GetGuideByIdAsync(Guid id);
    Task<GuideDto> CreateGuideAsync(CreateGuideRequest request);
    Task<bool> DeleteGuideAsync(Guid id);
}
