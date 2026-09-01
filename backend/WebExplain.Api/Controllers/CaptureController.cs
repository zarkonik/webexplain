using Microsoft.AspNetCore.Mvc;
using WebExplain.Api.DTOs;
using WebExplain.Api.Extensions;
using WebExplain.Api.Services;

namespace WebExplain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CaptureController(ICaptureService captureService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CaptureSessionDto>>> GetAll()
    {
        return Ok(await captureService.GetAllSessionsAsync(User.GetUserId()));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CaptureSessionDto>> GetById(Guid id)
    {
        var session = await captureService.GetSessionByIdAsync(id, User.GetUserId());
        return session is null ? NotFound() : Ok(session);
    }

    [HttpPost]
    public async Task<ActionResult<CaptureSessionDto>> Create(CreateCaptureRequest request, CancellationToken cancellationToken)
    {
        var result = await captureService.CaptureAsync(request, User.GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}/screenshot")]
    public Task<IActionResult> GetScreenshot(Guid id) => GetScreenshot(id, 1);

    [HttpGet("{id:guid}/screenshot/{order:int}")]
    public async Task<IActionResult> GetScreenshot(Guid id, int order)
    {
        var session = await captureService.GetSessionByIdAsync(id, User.GetUserId());
        var page = session?.Pages.FirstOrDefault(p => p.Order == order);
        if (page is null || !System.IO.File.Exists(page.ScreenshotFilePath))
            return NotFound();

        return PhysicalFile(page.ScreenshotFilePath, "image/png");
    }

    [HttpGet("{id:guid}/html")]
    public Task<IActionResult> GetHtml(Guid id) => GetHtml(id, 1);

    [HttpGet("{id:guid}/html/{order:int}")]
    public async Task<IActionResult> GetHtml(Guid id, int order)
    {
        var session = await captureService.GetSessionByIdAsync(id, User.GetUserId());
        var page = session?.Pages.FirstOrDefault(p => p.Order == order);
        if (page is null || !System.IO.File.Exists(page.HtmlFilePath))
            return NotFound();

        return PhysicalFile(page.HtmlFilePath, "text/html");
    }
}
