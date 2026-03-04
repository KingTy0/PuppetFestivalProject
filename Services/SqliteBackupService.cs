// File: Services/SqliteBackupService.cs
//
// PURPOSE: Core backup logic. Creates consistent SQLite snapshots using the
// SQLite Online Backup API and uploads them to Azure Blob Storage. Also
// handles restoring the database from the most recent backup blob.
//
// LIFETIME: Registered as Scoped in DI (one instance per request/scope).
// The BackupQueueService (Singleton) creates a scope to resolve this service.
//
// WHY SCOPED? Configuration values (like connection strings) are read fresh
// each time, which is important if they change via hot reload or env vars.

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Data.Sqlite;

namespace PuppetFestAPP.Web.Services;

/// <summary>
/// Provides backup and restore operations for the SQLite database.
/// Backups are stored as complete .db files in Azure Blob Storage.
/// Each backup is an atomic, consistent snapshot taken via the SQLite
/// Online Backup API — safe even while EF Core has active connections.
/// </summary>
public class SqliteBackupService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SqliteBackupService> _logger;
    private readonly string _dbPath;

    /// <summary>
    /// Initializes the backup service and resolves the SQLite database
    /// file path from the application's connection string.
    /// </summary>
    /// <param name="configuration">
    /// ASP.NET Core configuration (reads from appsettings.json and
    /// environment variables). Injected automatically by DI.
    /// </param>
    /// <param name="logger">Structured logger. Injected automatically by DI.</param>
    public SqliteBackupService(
        IConfiguration configuration,
        ILogger<SqliteBackupService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // ── Parse the database file path from the connection string ──
        // SQLite connection strings look like: "DataSource=app.db;Cache=Shared"
        // or "Data Source=/app/data/app.db;Cache=Shared" (with a space).
        // We split by semicolons to get individual key=value pairs, then find
        // the one starting with "DataSource=" or "Data Source=" to extract
        // the file path. Falls back to "app.db" if parsing fails.
        var connStr = _configuration
            .GetConnectionString("DefaultConnection")
            ?? "DataSource=app.db;Cache=Shared";
        _dbPath = connStr
            .Split(';')
            .Select(s => s.Trim())
            .FirstOrDefault(s =>
                s.StartsWith("DataSource=",
                    StringComparison.OrdinalIgnoreCase) ||
                s.StartsWith("Data Source=",
                    StringComparison.OrdinalIgnoreCase))
            ?.Split('=', 2)[1]?.Trim()
            ?? "app.db";
    }

    /// <summary>
    /// Returns true if Azure Blob Storage backup is both enabled in
    /// configuration AND has a valid connection string. When false,
    /// the timed backup service and manual backup button silently skip.
    /// </summary>
    public bool IsEnabled
    {
        get
        {
            var useStorage = _configuration
                .GetValue<bool>("Backup:UseStorage", false);
            var blobConnStr = _configuration["Backup:BlobConnectionString"];
            return useStorage && !string.IsNullOrWhiteSpace(blobConnStr);
        }
    }

    /// <summary>
    /// The resolved filesystem path to the SQLite database file
    /// (e.g., "app.db" or "/app/data/app.db" in Docker).
    /// Exposed publicly so the Program.cs startup logic can call
    /// File.Exists() to check whether a restore is needed BEFORE
    /// any DbContext is created.
    /// </summary>
    public string DatabasePath => _dbPath;

    /// <summary>
    /// Creates an atomic backup of the SQLite database and uploads it
    /// to Azure Blob Storage. Safe to call while the app is serving
    /// requests — the SQLite Online Backup API handles concurrency.
    /// </summary>
    /// <returns>The blob name (e.g., "app-db-2025-07-15_14-30-00.db").</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if backup is not enabled in configuration.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown if the SQLite database file doesn't exist on disk.
    /// </exception>
    public async Task<string> BackupAsync()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException(
                "Backup is not enabled. Set Backup:UseStorage=true " +
                "and provide Backup:BlobConnectionString.");
        }

        var blobConnStr = _configuration["Backup:BlobConnectionString"]!;
        var containerName = _configuration["Backup:ContainerName"]
            ?? "sqlite-backups";

        if (!File.Exists(_dbPath))
        {
            throw new FileNotFoundException(
                $"SQLite database not found at: {_dbPath}");
        }

        // Create a unique temp file for the backup snapshot.
        // We write to a temp file first (not directly to blob) because
        // the Backup API writes to a local SQLite database file.
        var tempBackupPath = Path.Combine(
            Path.GetTempPath(),
            $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.db");

        try
        {
            // ── SQLite Online Backup API ──
            // This is the ONLY safe way to back up an active SQLite database.
            // It creates an atomic, page-level snapshot of the entire database
            // even while other connections are reading or writing. The
            // alternative (File.Copy) can produce corrupt backups if a write
            // occurs during the copy.
            //
            // How it works internally:
            // 1. Opens a read-only connection to the live database
            // 2. Opens a connection to the (empty) destination file
            // 3. Copies all database pages atomically, retrying any pages
            //    that were modified during the copy
            // 4. The destination file is a valid, self-contained SQLite DB
            using (var source = new SqliteConnection(
                $"Data Source={_dbPath};Mode=ReadOnly"))
            using (var destination = new SqliteConnection(
                $"Data Source={tempBackupPath}"))
            {
                source.Open();
                destination.Open();
                source.BackupDatabase(destination);
            }
            // At this point, tempBackupPath contains a consistent snapshot.

            // ── Upload the snapshot to Azure Blob Storage ──
            var blobServiceClient = new BlobServiceClient(blobConnStr);
            var containerClient = blobServiceClient
                .GetBlobContainerClient(containerName);

            // CreateIfNotExistsAsync is safe to call every time — it's a
            // no-op if the container already exists.
            await containerClient
                .CreateIfNotExistsAsync(PublicAccessType.None);

            // Blob name includes a timestamp so backups are naturally
            // ordered by creation time. Lexicographic sort = chronological.
            var blobName =
                $"app-db-{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.db";
            var blobClient = containerClient.GetBlobClient(blobName);

            // Stream the file to Azure. BlobUploadOptions lets us set
            // the content type for proper identification in the portal.
            await using var stream = File.OpenRead(tempBackupPath);
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "application/x-sqlite3"
                }
            });

            _logger.LogInformation(
                "SQLite backup uploaded to blob: {BlobName}", blobName);
            return blobName;
        }
        finally
        {
            // Always clean up the temp file, even if the upload fails
            if (File.Exists(tempBackupPath))
            {
                File.Delete(tempBackupPath);
            }
        }
    }

    /// <summary>
    /// Restores the SQLite database by downloading the most recent
    /// backup blob and overwriting the local database file.
    ///
    /// ⚠️ WARNING: This clears ALL active SQLite connection pools,
    /// which means any in-flight database queries will fail. Active
    /// Blazor Server sessions may show errors. A full app restart
    /// is recommended after calling this method.
    /// </summary>
    /// <returns>The blob name that was restored from.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if backup is not enabled or no backup blobs exist.
    /// </exception>
    public async Task<string> RestoreLatestAsync()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException(
                "Backup is not enabled.");
        }

        var blobConnStr = _configuration["Backup:BlobConnectionString"]!;
        var containerName = _configuration["Backup:ContainerName"]
            ?? "sqlite-backups";

        var blobServiceClient = new BlobServiceClient(blobConnStr);
        var containerClient = blobServiceClient
            .GetBlobContainerClient(containerName);

        if (!await containerClient.ExistsAsync())
        {
            throw new InvalidOperationException(
                "Backup container does not exist.");
        }

        // ── Find the most recent backup blob ──
        // Blob names are timestamped (e.g., "app-db-2025-07-15_14-30-00.db")
        // so a simple string comparison finds the newest one.
        BlobItem? latestBlob = null;
        await foreach (var blob in containerClient.GetBlobsAsync(
    BlobTraits.None,
    BlobStates.None,
    prefix: "app-db-",
    cancellationToken: default))

        {
            if (latestBlob == null ||
                string.Compare(blob.Name, latestBlob.Name,
                    StringComparison.Ordinal) > 0)
            {
                latestBlob = blob;
            }
        }

        if (latestBlob == null)
        {
            throw new InvalidOperationException(
                "No backup blobs found.");
        }

        // ── Clear all SQLite connection pools ──
        // EF Core (and Microsoft.Data.Sqlite) cache open database
        // connections in a pool. These connections hold file-system locks
        // on app.db. If we try to overwrite the file while locks are held,
        // we get an IOException ("file in use"). ClearAllPools() closes
        // every pooled connection, releasing the locks.
        SqliteConnection.ClearAllPools();

        // ── Delete WAL/SHM journal files FIRST ──
        // SQLite uses Write-Ahead Logging (WAL) mode by default. This
        // creates two companion files: .db-wal (pending writes) and
        // .db-shm (shared memory index). These files belong to the OLD
        // database. If we write the new .db file first and something
        // reopens a SQLite connection before we delete these files,
        // SQLite will see the old -wal alongside the new .db and attempt
        // to replay stale WAL transactions — silently corrupting the
        // freshly restored database.
        //
        // By deleting WAL/SHM first, the worst case during the brief
        // window before the new .db is written is that a reconnecting
        // client sees the old .db without a WAL (valid, just stale).
        var walPath = _dbPath + "-wal";
        var shmPath = _dbPath + "-shm";
        if (File.Exists(walPath)) File.Delete(walPath);
        if (File.Exists(shmPath)) File.Delete(shmPath);

        // ── Download and overwrite the database file ──
        var blobClient = containerClient.GetBlobClient(latestBlob.Name);
        var downloadResult = await blobClient.DownloadContentAsync();

        await File.WriteAllBytesAsync(
            _dbPath,
            downloadResult.Value.Content.ToArray());

        _logger.LogInformation(
            "SQLite database restored from blob: {BlobName}",
            latestBlob.Name);
        return latestBlob.Name;
    }

    /// <summary>
    /// Restores the latest backup to a specific file path. This overload
    /// is used during app startup (in Program.cs) BEFORE any DbContext
    /// has been created, so there are no connection pools to clear and
    /// no file locks to worry about.
    /// </summary>
    /// <param name="targetPath">
    /// The filesystem path to write the restored database to
    /// (e.g., "/app/data/app.db").
    /// </param>
    /// <returns>The blob name that was restored from.</returns>
    public async Task<string> RestoreLatestToPathAsync(string targetPath)
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException(
                "Backup is not enabled.");
        }

        var blobConnStr = _configuration["Backup:BlobConnectionString"]!;
        var containerName = _configuration["Backup:ContainerName"]
            ?? "sqlite-backups";

        var blobServiceClient = new BlobServiceClient(blobConnStr);
        var containerClient = blobServiceClient
            .GetBlobContainerClient(containerName);

        if (!await containerClient.ExistsAsync())
        {
            throw new InvalidOperationException(
                "Backup container does not exist.");
        }

        // Same "find latest blob" logic as RestoreLatestAsync
        BlobItem? latestBlob = null;
       await foreach (var blob in containerClient.GetBlobsAsync(
    BlobTraits.None,
    BlobStates.None,
    prefix: "app-db-",
    cancellationToken: default))

        {
            if (latestBlob == null ||
                string.Compare(blob.Name, latestBlob.Name,
                    StringComparison.Ordinal) > 0)
            {
                latestBlob = blob;
            }
        }

        if (latestBlob == null)
        {
            throw new InvalidOperationException(
                "No backup blobs found.");
        }

        var blobClient = containerClient.GetBlobClient(latestBlob.Name);
        var downloadResult = await blobClient.DownloadContentAsync();

        await File.WriteAllBytesAsync(
            targetPath,
            downloadResult.Value.Content.ToArray());

        return latestBlob.Name;
    }
}