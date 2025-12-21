namespace NovaToolsHub.Services;

public interface ITempBatchStorage
{
    string CreateBatchDirectory(string batchId);
    string GetBatchDirectory(string batchId);
    bool BatchExists(string batchId);
    (int filesDeleted, long bytesFreed) DeleteBatch(string batchId);
    void CleanupExpiredBatches(TimeSpan ttl);
    bool IsSafeBatchId(string batchId);
}
