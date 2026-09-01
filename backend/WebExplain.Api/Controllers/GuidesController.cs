using Microsoft.AspNetCore.Mvc;
using WebExplain.Api.DTOs;
using WebExplain.Api.Extensions;
using WebExplain.Api.Services;

namespace WebExplain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuidesController(IGuideService guideService, IGuideExportService guideExportService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GuideDto>>> GetAll()
    {
        return Ok(await guideService.GetAllGuidesAsync(User.GetUserId()));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GuideDto>> GetById(Guid id)
    {
        var guide = await guideService.GetGuideByIdAsync(id, User.GetUserId());
        return guide is null ? NotFound() : Ok(guide);
    }

    [HttpPost]
    public async Task<ActionResult<GuideDto>> Create(CreateGuideRequest request)
    {
        var created = await guideService.CreateGuideAsync(request, User.GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await guideService.DeleteGuideAsync(id, User.GetUserId());
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id:guid}/export/word")]
    public async Task<IActionResult> ExportToWord(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var bytes = await guideExportService.ExportToWordAsync(id, userId, cancellationToken);
        if (bytes is null)
        {
            return NotFound();
        }

        var guide = await guideService.GetGuideByIdAsync(id, userId);
        var fileName = $"{SanitizeFileName(guide?.Title ?? "guide")}.docx";

        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(c => invalid.Contains(c) ? '-' : c).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "guide" : cleaned;
    }
}
