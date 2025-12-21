namespace NovaToolsHub.Services;

/// <summary>
/// Service for managing temporary file storage with automatic cleanup.
/// Used for storing processed images temporarily before download.
/// </summary>
public interface ITempResultStorage
{
    /// <summary>
    /// Stores binary data temporarily and returns a unique key for retrieval.
    /// </summary>
    /// <param name="data">The data to store.</param>
    /// <param name="extension">File extension (e.g., ".png").</param>
    /// <returns>A unique key for retrieving the data.</returns>
    string Store(byte[] data, string extension = ".png");

    /// <summary>
    /// Retrieves stored data by key.
    /// </summary>
    /// <param name="key">The storage key.</param>
    /// <returns>The stored data, or null if not found/expired.</returns>
    byte[]? Retrieve(string key);

    /// <summary>
    /// Retrieves stored data as a stream.
    /// </summary>
    /// <param name="key">The storage key.</param>
    /// <returns>A stream to the data, or null if not found/expired.</returns>
    Stream? RetrieveStream(string key);

    /// <summary>
    /// Retrieves stored data as a stream and deletes the file after the stream is closed.
    /// Used for download-once scenarios to free disk space.
    /// </summary>
    /// <param name="key">The storage key.</param>
    /// <returns>A stream that will delete the file on close, or null if not found/expired.</returns>
    Stream? RetrieveAndDeleteStream(string key);

    /// <summary>
    /// Deletes stored data by key.
    /// </summary>
    /// <param name="key">The storage key.</param>
    void Delete(string key);

    /// <summary>
    /// Cleans up expired files.
    /// </summary>
    void CleanupExpired();
}

/// <summary>
/// File-based implementation of temporary result storage.
/// Stores files in a temp directory with TTL-based cleanup.
/// </summary>
public class TempResultStorage : ITempResultStorage, IDisposable
{
    private readonly string _basePath;
    private readonly TimeSpan _ttl;
    private readonly ILogger<TempResultStorage> _logger;
    private readonly Timer _cleanupTimer;
    private bool _disposed;

    public TempResultStorage(ILogger<TempResultStorage> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _basePath = Path.Combine(environment.ContentRootPath, "App_Data", "temp_results");
        _ttl = TimeSpan.FromMinutes(30);

        // Ensure directory exists
        Directory.CreateDirectory(_basePath);

        // Start cleanup timer (runs every 5 minutes)
        _cleanupTimer = new Timer(_ => CleanupExpired(), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

        _logger.LogInformation("TempResultStorage initialized at {Path} with TTL {TTL}", _basePath, _ttl);
    }

    public string Store(byte[] data, string extension = ".png")
    {
        var key = Guid.NewGuid().ToString("N");
        var filename = $"{key}{extension}";
        var filepath = Path.Combine(_basePath, filename);

        File.WriteAllBytes(filepath, data);
        _logger.LogDebug("Stored temp file: {Key} ({Size} bytes)", key, data.Length);

        return key;
    }

    public byte[]? Retrieve(string key)
    {
        var filepath = FindFile(key);
        if (filepath == null) return null;

        return File.ReadAllBytes(filepath);
    }

    public Stream? RetrieveStream(string key)
    {
        var filepath = FindFile(key);
        if (filepath == null) return null;

        return new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    /// <summary>
    /// Returns a stream that deletes the file when closed (delete-on-download).
    /// </summary>
    public Stream? RetrieveAndDeleteStream(string key)
    {
        var filepath = FindFile(key);
        if (filepath == null) return null;

        // Return a stream wrapper that deletes on dispose
        return new DeleteOnCloseFileStream(filepath, _logger);
    }

    public void Delete(string key)
    {
        var filepath = FindFile(key);
        if (filepath == null) return;

        try
        {
            File.Delete(filepath);
            _logger.LogDebug("Deleted temp file: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete temp file: {Key}", key);
        }
    }

    public void CleanupExpired()
    {
        if (!Directory.Exists(_basePath)) return;

        var cutoff = DateTime.UtcNow - _ttl;
        var files = Directory.GetFiles(_basePath);
        var deleted = 0;

        foreach (var file in files)
        {
            try
            {
                var info = new FileInfo(file);
                if (info.CreationTimeUtc < cutoff)
                {
                    File.Delete(file);
                    deleted++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup temp file: {File}", file);
            }
        }

        if (deleted > 0)
        {
            _logger.LogInformation("Cleaned up {Count} expired temp files", deleted);
        }
    }

    private string? FindFile(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;

        // Sanitize key to prevent path traversal
        if (key.Contains("..") || key.Contains(Path.DirectorySeparatorChar) || key.Contains(Path.AltDirectorySeparatorChar))
        {
            _logger.LogWarning("Invalid key format detected: {Key}", key);
            return null;
        }

        var pattern = $"{key}.*";
        var files = Directory.GetFiles(_basePath, pattern);

        if (files.Length == 0) return null;

        var filepath = files[0];
        var info = new FileInfo(filepath);

        // Check if expired
        if (info.CreationTimeUtc < DateTime.UtcNow - _ttl)
        {
            try { File.Delete(filepath); } catch { }
            return null;
        }

        return filepath;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cleanupTimer.Dispose();
    }
}

/// <summary>
/// A FileStream wrapper that deletes the underlying file when closed/disposed.
/// Used for delete-after-download scenarios.
/// </summary>
internal class DeleteOnCloseFileStream : FileStream
{
    private readonly string _filePath;
    private readonly ILogger _logger;
    private bool _deleted;

    public DeleteOnCloseFileStream(string path, ILogger logger)
        : base(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, FileOptions.DeleteOnClose)
    {
        _filePath = path;
        _logger = logger;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        // FileOptions.DeleteOnClose handles the deletion, but log it
        if (!_deleted)
        {
            _deleted = true;
            _logger.LogDebug("Temp file deleted after download: {Path}", Path.GetFileName(_filePath));
        }
    }
}
