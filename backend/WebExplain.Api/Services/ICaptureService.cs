using WebExplain.Api.DTOs;

namespace WebExplain.Api.Services;

public interface ICaptureService
{
    Task<List<CaptureSessionDto>> GetAllSessionsAsync(Guid userId);
    Task<CaptureSessionDto?> GetSessionByIdAsync(Guid id, Guid userId);
    Task<CaptureSessionDto> CaptureAsync(CreateCaptureRequest request, Guid userId, CancellationToken cancellationToken = default);
}
