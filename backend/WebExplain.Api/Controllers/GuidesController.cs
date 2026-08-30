using Microsoft.AspNetCore.Mvc;
using WebExplain.Api.DTOs;
using WebExplain.Api.Services;

namespace WebExplain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuidesController(IGuideService guideService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GuideDto>>> GetAll()
    {
        return Ok(await guideService.GetAllGuidesAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GuideDto>> GetById(Guid id)
    {
        var guide = await guideService.GetGuideByIdAsync(id);
        return guide is null ? NotFound() : Ok(guide);
    }

    [HttpPost]
    public async Task<ActionResult<GuideDto>> Create(CreateGuideRequest request)
    {
        var created = await guideService.CreateGuideAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await guideService.DeleteGuideAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
