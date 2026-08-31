namespace WebExplain.Api.Services;

public interface IGuideExportService
{
    Task<byte[]?> ExportToWordAsync(Guid guideId, CancellationToken cancellationToken = default);
}
