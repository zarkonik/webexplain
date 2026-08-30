using Microsoft.EntityFrameworkCore;
using WebExplain.Api.Models;

namespace WebExplain.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Guide> Guides => Set<Guide>();
    public DbSet<GuideStep> GuideSteps => Set<GuideStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Guide>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.HasMany(g => g.Steps)
                .WithOne(s => s.Guide)
                .HasForeignKey(s => s.GuideId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GuideStep>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => new { s.GuideId, s.Order });
        });
    }
}
