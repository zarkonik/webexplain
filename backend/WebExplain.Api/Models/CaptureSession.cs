namespace WebExplain.Api.Models;

public enum CaptureStatus
{
    Pending,
    Running,
    Completed,
    Failed
}

public class CaptureSession
{
    public Guid Id { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public CaptureStatus Status { get; set; } = CaptureStatus.Pending;
    public string? ErrorMessage { get; set; }
    public string StorageFolder { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public List<CapturedPage> Pages { get; set; } = [];
}
