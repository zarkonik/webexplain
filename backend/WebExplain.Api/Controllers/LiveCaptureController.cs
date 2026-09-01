using Microsoft.AspNetCore.Mvc;
using WebExplain.Api.DTOs;
using WebExplain.Api.Extensions;
using WebExplain.Api.Services.LiveCapture;

namespace WebExplain.Api.Controllers;

[ApiController]
[Route("api/live-capture")]
public class LiveCaptureController(ILiveCaptureManager manager) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<StartLiveCaptureResponse>> Start(StartLiveCaptureRequest request, CancellationToken cancellationToken)
    {
        var result = await manager.StartAsync(request.Url, User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{sessionId:guid}/inspect")]
    public async Task<ActionResult<LiveCaptureInspectResponse>> Inspect(
        Guid sessionId, LiveCaptureInspectRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var probe = await manager.InspectAsync(sessionId, User.GetUserId(), request.XRatio, request.YRatio, cancellationToken);
            return Ok(new LiveCaptureInspectResponse(probe.Selector, probe.IsFillable));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{sessionId:guid}/click")]
    public async Task<ActionResult<LiveCaptureStepResponse>> Click(
        Guid sessionId, LiveCaptureClickRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await manager.ClickAsync(sessionId, User.GetUserId(), request.XRatio, request.YRatio, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{sessionId:guid}/fill")]
    public async Task<ActionResult<LiveCaptureStepResponse>> Fill(
        Guid sessionId, LiveCaptureFillRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await manager.FillAsync(sessionId, User.GetUserId(), request.XRatio, request.YRatio, request.Value, cancellationToken);
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
        var path = manager.GetScreenshotPath(sessionId, User.GetUserId(), order);
        if (path is null || !System.IO.File.Exists(path))
            return NotFound();

        // The file behind the current step's order can be overwritten in place by a
        // scroll action, so this response must never be cached by the browser.
        Response.Headers.CacheControl = "no-store";
        return PhysicalFile(path, "image/png");
    }

    [HttpPost("{sessionId:guid}/scroll")]
    public async Task<ActionResult<LiveCaptureScrollResponse>> Scroll(
        Guid sessionId, LiveCaptureScrollRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await manager.ScrollAsync(sessionId, User.GetUserId(), request.DeltaY, cancellationToken);
            return Ok(new LiveCaptureScrollResponse(order));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{sessionId:guid}/finish")]
    public async Task<ActionResult<List<RecordedStepDto>>> Finish(Guid sessionId, CancellationToken cancellationToken)
    {
        try
        {
            var steps = await manager.FinishAsync(sessionId, User.GetUserId(), cancellationToken);
            var dtos = steps.Select(s => new RecordedStepDto(
                s.Order, s.ActionType, s.Selector, s.Value, s.ElementDescription, s.Url,
                s.TargetX, s.TargetY, s.TargetWidth, s.TargetHeight)).ToList();
            return Ok(dtos);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
