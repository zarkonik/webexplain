using Microsoft.EntityFrameworkCore;
using WebExplain.Api.Data;
using WebExplain.Api.Models;

namespace WebExplain.Api.Repositories;

public class GuideRepository(ApplicationDbContext context) : IGuideRepository
{
    public async Task<List<Guide>> GetAllAsync()
    {
        return await context.Guides
            .Include(g => g.Steps.OrderBy(s => s.Order))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<Guide?> GetByIdAsync(Guid id)
    {
        return await context.Guides
            .Include(g => g.Steps.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Guide> AddAsync(Guide guide)
    {
        context.Guides.Add(guide);
        await context.SaveChangesAsync();
        return guide;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var guide = await context.Guides.FindAsync(id);
        if (guide is null) return false;

        context.Guides.Remove(guide);
        await context.SaveChangesAsync();
        return true;
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
