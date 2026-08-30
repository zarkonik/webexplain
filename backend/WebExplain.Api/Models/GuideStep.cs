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
}
