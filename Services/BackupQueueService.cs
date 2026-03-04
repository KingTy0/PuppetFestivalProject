// File: Services/BackupQueueService.cs
//
// PURPOSE: Serializes all backup operations through a single queue so that:
//   1. Only one backup runs at a time (prevents redundant uploads)
//   2. Any part of the app can trigger a backup after a write operation
//   3. Callers don't have to wait for the backup to complete
//
// HOW IT WORKS:
//   - Uses a SemaphoreSlim(1, 1) as a mutex (mutual exclusion lock).
//     Think of it as a single-item "slot": if the slot is occupied,
//     new requests skip instead of queuing up.
//   - EnqueueBackupAsync() is fire-and-forget: it starts the backup on
//     a background thread and returns immediately. If a backup is already
//     running, the new request is silently discarded (not queued).
//   - This is safe because backups are full-database snapshots. If two
//     writes happen 10ms apart, only one backup needs to run — the second
//     one would capture a near-identical database state.
//
// LIFETIME: Registered as a Singleton in DI (one instance for the entire app).
// This is essential because the SemaphoreSlim must be shared across all
// callers (timed backup, manual backup, write-triggered backup).

namespace PuppetFestAPP.Web.Services;

/// <summary>
/// Singleton service that serializes backup operations. Ensures only one
/// backup runs at a time and provides a fire-and-forget entry point that
/// any service or component can call after performing a database write.
/// </summary>
public class BackupQueueService
{
    // ── The mutex ──
    // SemaphoreSlim(1, 1) acts as an async-compatible lock:
    //   - initialCount: 1 = one "permit" available (unlocked)
    //   - maxCount: 1 = maximum one permit total
    // When a backup starts, it takes the permit (count goes to 0).
    // When it finishes, it releases the permit (count goes back to 1).
    // If another backup tries to start while count is 0, WaitAsync(0)
    // returns false immediately (non-blocking) and we skip the backup.
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackupQueueService> _logger;

    /// <summary>
    /// Initializes the backup queue with access to the DI container.
    /// </summary>
    /// <param name="serviceProvider">
    /// The root service provider. We use this to create scoped instances
    /// of SqliteBackupService on demand (since it's registered as Scoped).
    /// </param>
    /// <param name="logger">Structured logger for backup status messages.</param>
    public BackupQueueService(
        IServiceProvider serviceProvider,
        ILogger<BackupQueueService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Triggers a backup on a background thread and returns immediately.
    /// If a backup is already in progress, this call is silently skipped
    /// (no error, no queuing). This is the primary method to call after
    /// any database write operation.
    ///
    /// <para><b>Example usage in a Blazor component or service:</b></para>
    /// <code>
    /// @inject BackupQueueService BackupQueue
    ///
    /// // After saving a sale:
    /// await DbContext.SaveChangesAsync();
    /// BackupQueue.EnqueueBackup();
    /// </code>
    /// </summary>
    public void EnqueueBackup()
    {
        // Fire-and-forget: start the backup task but don't await it.
        // The underscore (_) discards the Task to suppress the compiler
        // warning about un-awaited tasks. The actual error handling
        // happens inside RunBackupAsync.
        _ = RunBackupAsync();
    }

    /// <summary>
    /// Runs a backup, respecting the semaphore to ensure only one backup
    /// executes at a time. If the semaphore is already held (a backup is
    /// running), this method returns immediately without doing anything.
    /// </summary>
    /// <returns>A task that completes when the backup finishes or is skipped.</returns>
    private async Task RunBackupAsync()
    {
        // ── Try to acquire the lock without waiting ──
        // WaitAsync(TimeSpan.Zero) returns true if we got the lock,
        // false if it's already held. This is non-blocking — we never
        // wait in a queue, we just skip if busy.
        var acquired = await _semaphore.WaitAsync(TimeSpan.Zero);

        if (!acquired)
        {
            // Another backup is already running. Since backups are full
            // snapshots, the running backup will capture our write too
            // (or very close to it). Safe to skip.
            _logger.LogDebug(
                "Backup already in progress, skipping enqueued backup");
            return;
        }

        try
        {
            // ── Create a DI scope to resolve the Scoped backup service ──
            // SqliteBackupService is registered as Scoped, meaning it can't
            // be resolved directly from the root provider. We create a short-
            // lived scope that provides the correct service lifetime.
            using var scope = _serviceProvider.CreateScope();
            var backupService = scope.ServiceProvider
                .GetRequiredService<SqliteBackupService>();

            if (backupService.IsEnabled)
            {
                var blobName = await backupService.BackupAsync();
                _logger.LogInformation(
                    "Queued backup completed: {BlobName}", blobName);
            }
        }
        catch (Exception ex)
        {
            // Log but don't throw — this runs on a fire-and-forget thread.
            // Throwing here would cause an unobserved task exception.
            _logger.LogError(ex, "Queued backup failed");
        }
        finally
        {
            // ── Always release the lock ──
            // This is in a finally block so the lock is released even if
            // the backup throws an exception. Without this, a failed backup
            // would permanently block all future backups.
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Runs a backup and waits for it to complete. Unlike
    /// <see cref="EnqueueBackup"/>, this method blocks until the backup
    /// finishes. Used by the timed backup service and the manual
    /// "Backup Now" button where you want confirmation of completion.
    /// </summary>
    /// <returns>The blob name if a backup was performed, or null if skipped.</returns>
    public async Task<string?> RunBackupAndWaitAsync()
    {
        var acquired = await _semaphore.WaitAsync(TimeSpan.Zero);
        if (!acquired)
        {
            _logger.LogDebug(
                "Backup already in progress, skipping");
            return null;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var backupService = scope.ServiceProvider
                .GetRequiredService<SqliteBackupService>();

            if (backupService.IsEnabled)
            {
                var blobName = await backupService.BackupAsync();
                _logger.LogInformation(
                    "Backup completed: {BlobName}", blobName);
                return blobName;
            }

            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}