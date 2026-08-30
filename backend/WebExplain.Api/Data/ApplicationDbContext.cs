using Microsoft.EntityFrameworkCore;
using WebExplain.Api.Models;

namespace WebExplain.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Guide> Guides => Set<Guide>();
    public DbSet<GuideStep> GuideSteps => Set<GuideStep>();
    public DbSet<CaptureSession> CaptureSessions => Set<CaptureSession>();
    public DbSet<CapturedPage> CapturedPages => Set<CapturedPage>();

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

        modelBuilder.Entity<CaptureSession>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Status).HasConversion<string>();
            entity.HasMany(c => c.Pages)
                .WithOne(p => p.CaptureSession)
                .HasForeignKey(p => p.CaptureSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CapturedPage>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => new { p.CaptureSessionId, p.Order });
        });
    }
}
