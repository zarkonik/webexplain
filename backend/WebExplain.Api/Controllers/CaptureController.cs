using Microsoft.AspNetCore.Mvc;
using WebExplain.Api.DTOs;
using WebExplain.Api.Services;

namespace WebExplain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CaptureController(ICaptureService captureService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CaptureSessionDto>>> GetAll()
    {
        return Ok(await captureService.GetAllSessionsAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CaptureSessionDto>> GetById(Guid id)
    {
        var session = await captureService.GetSessionByIdAsync(id);
        return session is null ? NotFound() : Ok(session);
    }

    [HttpPost]
    public async Task<ActionResult<CaptureSessionDto>> Create(CreateCaptureRequest request, CancellationToken cancellationToken)
    {
        var result = await captureService.CaptureAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
