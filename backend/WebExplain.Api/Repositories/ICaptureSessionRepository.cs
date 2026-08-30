using WebExplain.Api.Models;

namespace WebExplain.Api.Repositories;

public interface ICaptureSessionRepository
{
    Task<List<CaptureSession>> GetAllAsync();
    Task<CaptureSession?> GetByIdAsync(Guid id);
    Task<CaptureSession> AddAsync(CaptureSession session);
    Task<CapturedPage> AddPageAsync(CapturedPage page);
    Task SaveChangesAsync();
}
