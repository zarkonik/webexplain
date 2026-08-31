namespace WebExplain.Api.Models;

public class GuideStep
{
    public Guid Id { get; set; }
    public Guid GuideId { get; set; }
    public Guide? Guide { get; set; }

    public int Order { get; set; }
    public string TargetSelector { get; set; } = string.Empty;
    public string Instruction { get; set; } = string.Empty;
    public string ActionType { get; set; } = "click"; // click, fill, navigate, highlight
    public string? InputValue { get; set; }
    public string? PageUrl { get; set; }
    public string? ElementDescription { get; set; }

    /// <summary>
    /// Viewport-relative bounding box (in the 1280x800 capture viewport's CSS pixels) of the
    /// element this step acted on - used to draw a highlight over the *previous* step's
    /// screenshot, since that's the "before" state where the action was actually performed.
    /// Null for steps with no associated element (e.g. the initial "navigate" step).
    /// </summary>
    public double? TargetX { get; set; }
    public double? TargetY { get; set; }
    public double? TargetWidth { get; set; }
    public double? TargetHeight { get; set; }
}
