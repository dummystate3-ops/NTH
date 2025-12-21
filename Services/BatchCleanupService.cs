using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NovaToolsHub.Services;

/// <summary>
/// Background service that periodically cleans up expired batch directories
/// </summary>
public class BatchCleanupService : BackgroundService
{
    private readonly ITempBatchStorage _tempBatchStorage;
    private readonly ILogger<BatchCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(10);
    private readonly TimeSpan _batchTtl = TimeSpan.FromMinutes(30);

    public BatchCleanupService(
        ITempBatchStorage tempBatchStorage,
        ILogger<BatchCleanupService> logger)
    {
        _tempBatchStorage = tempBatchStorage;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BatchCleanupService started. Cleanup interval: {Interval}", _cleanupInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);

                _logger.LogDebug("Running batch cleanup...");
                _tempBatchStorage.CleanupExpiredBatches(_batchTtl);
            }
            catch (OperationCanceledException)
            {
                // Expected when service is stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch cleanup");
            }
        }

        _logger.LogInformation("BatchCleanupService stopped.");
    }
}
