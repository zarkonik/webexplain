using WebExplain.Api.Models;

namespace WebExplain.Api.Repositories;

public interface ICaptureSessionRepository
{
    Task<List<CaptureSession>> GetAllAsync(Guid userId);
    Task<CaptureSession?> GetByIdAsync(Guid id, Guid userId);
    Task<CaptureSession> AddAsync(CaptureSession session);
    Task<CapturedPage> AddPageAsync(CapturedPage page);
    Task SaveChangesAsync();
}
