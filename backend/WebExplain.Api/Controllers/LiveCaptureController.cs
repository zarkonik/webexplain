using Microsoft.AspNetCore.Mvc;
using WebExplain.Api.DTOs;
using WebExplain.Api.Services.LiveCapture;

namespace WebExplain.Api.Controllers;

[ApiController]
[Route("api/live-capture")]
public class LiveCaptureController(ILiveCaptureManager manager) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<StartLiveCaptureResponse>> Start(StartLiveCaptureRequest request, CancellationToken cancellationToken)
    {
        var result = await manager.StartAsync(request.Url, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{sessionId:guid}/click")]
    public async Task<ActionResult<LiveCaptureStepResponse>> Click(
        Guid sessionId, LiveCaptureClickRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await manager.ClickAsync(sessionId, request.XRatio, request.YRatio, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{sessionId:guid}/screenshot/{order:int}")]
    public IActionResult GetScreenshot(Guid sessionId, int order)
    {
        var path = manager.GetScreenshotPath(sessionId, order);
        if (path is null || !System.IO.File.Exists(path))
            return NotFound();

        return PhysicalFile(path, "image/png");
    }

    [HttpPost("{sessionId:guid}/finish")]
    public async Task<ActionResult<List<RecordedStepDto>>> Finish(Guid sessionId, CancellationToken cancellationToken)
    {
        try
        {
            var steps = await manager.FinishAsync(sessionId, cancellationToken);
            var dtos = steps.Select(s => new RecordedStepDto(s.Order, s.ActionType, s.Selector, s.Url)).ToList();
            return Ok(dtos);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
