using WebExplain.Api.Models;

namespace WebExplain.Api.Repositories;

public interface IGuideRepository
{
    Task<List<Guide>> GetAllAsync(Guid userId);
    Task<Guide?> GetByIdAsync(Guid id, Guid userId);
    Task<Guide> AddAsync(Guide guide);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task SaveChangesAsync();
}
