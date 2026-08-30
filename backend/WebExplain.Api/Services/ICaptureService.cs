using WebExplain.Api.DTOs;

namespace WebExplain.Api.Services;

public interface ICaptureService
{
    Task<List<CaptureSessionDto>> GetAllSessionsAsync();
    Task<CaptureSessionDto?> GetSessionByIdAsync(Guid id);
    Task<CaptureSessionDto> CaptureAsync(CreateCaptureRequest request, CancellationToken cancellationToken = default);
}
