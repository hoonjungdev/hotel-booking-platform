namespace HotelBooking.Worker;

/// <summary>
/// Hosts background processing until module-specific workers are introduced.
/// </summary>
/// <param name="logger">The worker logger.</param>
public class Worker(ILogger<Worker> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Hotel Booking worker started.");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            logger.LogDebug("Hotel Booking worker heartbeat at {Time}.", TimeProvider.System.GetUtcNow());
        }
    }
}
