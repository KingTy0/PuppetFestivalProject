namespace PuppetFestAPP.Web.Services;

/// <summary>
/// Background service that triggers periodic SQLite backups.
/// Uses <see cref="BackupQueueService"/> to serialize with other
/// backup triggers (write-triggered, manual) and avoid redundant uploads.
/// </summary>
public class TimedBackupService : BackgroundService
{
    private readonly BackupQueueService _backupQueue;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TimedBackupService> _logger;

    /// <summary>
    /// Initializes the timed backup service with its dependencies.
    /// </summary>
    /// <param name="backupQueue">
    /// The singleton backup queue that serializes all backup operations.
    /// </param>
    /// <param name="configuration">App configuration for reading the interval.</param>
    /// <param name="logger">Structured logger.</param>
    public TimedBackupService(
        BackupQueueService backupQueue,
        IConfiguration configuration,
        ILogger<TimedBackupService> logger)
    {
        _backupQueue = backupQueue;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// The main loop that ASP.NET Core calls when the app starts.
    /// Runs until the app shuts down (signaled via stoppingToken).
    /// </summary>
    /// <param name="stoppingToken">
    /// Cancellation token that fires when the app is shutting down.
    /// We pass it to Task.Delay so the loop exits promptly on shutdown.
    /// </param>
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        // Read the backup interval from configuration.
        // Default: 5 minutes. Configurable in appsettings.json or via
        // environment variable Backup__IntervalMinutes.
        var intervalMinutes = _configuration
            .GetValue<int>("Backup:IntervalMinutes", 5);

        // Wait 30 seconds at startup before the first backup attempt.
        // This gives the app time to fully initialize (run migrations,
        // seed data, establish SignalR connections, etc.).
        _logger.LogInformation(
            "Timed backup service started. " +
            "Interval: {Interval} minutes. First backup in 30 seconds.",
            intervalMinutes);
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        // ── Main timer loop ──
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Route through the queue — if a write-triggered backup
                // is already running, this call will skip gracefully.
                var blobName = await _backupQueue.RunBackupAndWaitAsync();
                if (blobName != null)
                {
                    _logger.LogInformation(
                        "Timed backup completed: {BlobName}", blobName);
                }
            }
            catch (Exception ex)
            {
                // Log the error but keep the loop running. A single
                // failed backup shouldn't stop future backup attempts.
                _logger.LogError(ex, "Timed backup failed");
            }

            // Wait for the configured interval before the next backup.
            // Task.Delay with a CancellationToken throws
            // OperationCanceledException on shutdown, which exits the loop.
            await Task.Delay(
                TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }
}