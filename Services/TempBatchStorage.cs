using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace NovaToolsHub.Services;

public class TempBatchStorage : ITempBatchStorage
{
    private readonly string _basePath;
    private readonly ILogger<TempBatchStorage> _logger;

    public TempBatchStorage(ILogger<TempBatchStorage> logger)
    {
        _logger = logger;
        _basePath = Path.Combine(Path.GetTempPath(), "NovaToolsHub", "ImageBatches");
        Directory.CreateDirectory(_basePath);
    }

    public bool IsSafeBatchId(string batchId)
    {
        if (string.IsNullOrWhiteSpace(batchId)) return false;
        return !batchId.Any(c => Path.GetInvalidFileNameChars().Contains(c));
    }

    public string CreateBatchDirectory(string batchId)
    {
        if (!IsSafeBatchId(batchId))
        {
            throw new ArgumentException("Invalid batch ID", nameof(batchId));
        }

        var dir = GetBatchDirectory(batchId);
        Directory.CreateDirectory(dir);
        return dir;
    }

    public string GetBatchDirectory(string batchId)
    {
        if (!IsSafeBatchId(batchId))
        {
            throw new ArgumentException("Invalid batch ID", nameof(batchId));
        }

        return Path.Combine(_basePath, batchId);
    }

    public bool BatchExists(string batchId)
    {
        if (!IsSafeBatchId(batchId)) return false;
        return Directory.Exists(GetBatchDirectory(batchId));
    }

    public (int filesDeleted, long bytesFreed) DeleteBatch(string batchId)
    {
        if (!IsSafeBatchId(batchId)) return (0, 0);

        var dir = GetBatchDirectory(batchId);
        if (!Directory.Exists(dir)) return (0, 0);

        try
        {
            // Calculate stats before deletion
            var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            int fileCount = files.Length;
            long totalBytes = files.Sum(f => new FileInfo(f).Length);

            Directory.Delete(dir, true);
            return (fileCount, totalBytes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete batch directory {Directory}", dir);
            return (0, 0);
        }
    }

    public void CleanupExpiredBatches(TimeSpan ttl)
    {
        try
        {
            if (!Directory.Exists(_basePath)) return;

            var threshold = DateTime.UtcNow - ttl;
            foreach (var dir in Directory.EnumerateDirectories(_basePath))
            {
                try
                {
                    var info = new DirectoryInfo(dir);
                    var lastWrite = info.LastWriteTimeUtc;
                    if (lastWrite < threshold)
                    {
                        info.Delete(true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed cleaning up batch directory {Directory}", dir);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CleanupExpiredBatches failed");
        }
    }
}
