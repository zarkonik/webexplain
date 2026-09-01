namespace WebExplain.Api.Services;

public interface IGuideExportService
{
    Task<byte[]?> ExportToWordAsync(Guid guideId, Guid userId, CancellationToken cancellationToken = default);
}
