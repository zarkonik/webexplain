using WebExplain.Api.Models;

namespace WebExplain.Api.Repositories;

public interface IGuideRepository
{
    Task<List<Guide>> GetAllAsync();
    Task<Guide?> GetByIdAsync(Guid id);
    Task<Guide> AddAsync(Guide guide);
    Task<bool> DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
