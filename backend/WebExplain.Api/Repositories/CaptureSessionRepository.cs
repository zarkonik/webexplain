using Microsoft.EntityFrameworkCore;
using WebExplain.Api.Data;
using WebExplain.Api.Models;

namespace WebExplain.Api.Repositories;

public class CaptureSessionRepository(ApplicationDbContext context) : ICaptureSessionRepository
{
    public async Task<List<CaptureSession>> GetAllAsync(Guid userId)
    {
        return await context.CaptureSessions
            .Where(c => c.UserId == userId)
            .Include(c => c.Pages.OrderBy(p => p.Order))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<CaptureSession?> GetByIdAsync(Guid id, Guid userId)
    {
        return await context.CaptureSessions
            .Include(c => c.Pages.OrderBy(p => p.Order))
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    public async Task<CaptureSession> AddAsync(CaptureSession session)
    {
        context.CaptureSessions.Add(session);
        await context.SaveChangesAsync();
        return session;
    }

    public async Task<CapturedPage> AddPageAsync(CapturedPage page)
    {
        context.CapturedPages.Add(page);
        await context.SaveChangesAsync();
        return page;
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
