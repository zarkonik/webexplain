namespace WebExplain.Api.Services.LiveCapture;

public class LiveCaptureCleanupService(ILiveCaptureManager manager) : BackgroundService
{
    private static readonly TimeSpan IdleThreshold = TimeSpan.FromMinutes(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await manager.ExpireIdleSessionsAsync(IdleThreshold);
            }
            catch
            {
                // A failed sweep shouldn't stop future sweeps from running.
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
