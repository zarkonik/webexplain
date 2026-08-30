using WebExplain.Api.DTOs;
using WebExplain.Api.Models;
using WebExplain.Api.Repositories;
using WebExplain.Api.Services.Capture;

namespace WebExplain.Api.Services;

public class CaptureService(
    ICaptureSessionRepository repository,
    IBrowserCaptureEngine captureEngine,
    IWebHostEnvironment environment) : ICaptureService
{
    private string StorageRoot => Path.Combine(environment.ContentRootPath, "Storage", "Captures");

    public async Task<List<CaptureSessionDto>> GetAllSessionsAsync()
    {
        var sessions = await repository.GetAllAsync();
        return sessions.Select(ToDto).ToList();
    }

    public async Task<CaptureSessionDto?> GetSessionByIdAsync(Guid id)
    {
        var session = await repository.GetByIdAsync(id);
        return session is null ? null : ToDto(session);
    }

    public async Task<CaptureSessionDto> CaptureAsync(CreateCaptureRequest request, CancellationToken cancellationToken = default)
    {
        var session = new CaptureSession
        {
            Id = Guid.NewGuid(),
            SourceUrl = request.Url,
            Status = CaptureStatus.Running
        };
        session.StorageFolder = Path.Combine(StorageRoot, session.Id.ToString());

        await repository.AddAsync(session);

        try
        {
            var steps = (request.Steps ?? [])
                .Select(s => new CaptureStepAction(s.ActionType, s.Selector, s.Value))
                .ToList();

            var results = await captureEngine.CaptureAsync(request.Url, steps, session.StorageFolder, cancellationToken);

            for (var i = 0; i < results.Count; i++)
            {
                var result = results[i];
                await repository.AddPageAsync(new CapturedPage
                {
                    Id = Guid.NewGuid(),
                    CaptureSessionId = session.Id,
                    Order = i + 1,
                    Url = result.Url,
                    HtmlFilePath = result.HtmlFilePath,
                    ScreenshotFilePath = result.ScreenshotFilePath
                });
            }

            session.Status = CaptureStatus.Completed;
            session.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            session.Status = CaptureStatus.Failed;
            session.ErrorMessage = ex.Message;
        }

        await repository.SaveChangesAsync();
        return ToDto(session);
    }

    private static CaptureSessionDto ToDto(CaptureSession session) => new(
        session.Id,
        session.SourceUrl,
        session.Status,
        session.ErrorMessage,
        session.CreatedAt,
        session.CompletedAt,
        session.Pages.Select(p => new CapturedPageDto(
            p.Id, p.Order, p.Url, p.HtmlFilePath, p.ScreenshotFilePath, p.CapturedAt
        )).ToList()
    );
}
