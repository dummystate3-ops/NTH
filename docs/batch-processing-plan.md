# Batch Processing - Bug Fix & Improvement Plan

## Current Status
The user has refined the batch processing architecture with an `ITempBatchStorage` abstraction. The implementation is solid, but there's a runtime bug causing crashes.

---

## ⚠️ Critical Bug: ZIP File Already Exists

**Error:**
```
System.IO.IOException: The file 'C:\Users\...\nova_batch_xxx.zip' already exists.
```

**Root Cause:**
In `DownloadBatch`, when a user clicks "Download ZIP" multiple times, `ZipFile.CreateFromDirectory` fails because the target ZIP file was created on the first click but `FileOptions.DeleteOnClose` only deletes the file when the *last* stream handle closes. If the download is interrupted or the user clicks again quickly, the file persists.

**Fix:**
Delete the existing ZIP file before calling `CreateFromDirectory`:

```diff
 var tempZip = Path.Combine(Path.GetTempPath(), $"nova_batch_{batchId}.zip");
+
+// Delete stale zip if it exists (previous download attempt)
+if (System.IO.File.Exists(tempZip))
+{
+    try { System.IO.File.Delete(tempZip); } catch { /* ignore */ }
+}

 ZipFile.CreateFromDirectory(tempDir, tempZip);
```

**File:** `Controllers/ImageToolsController.cs` around line 549-552.

---

## Architecture Summary

### New Components
| File | Purpose |
|------|---------|
| `Services/ITempBatchStorage.cs` | Interface for temp batch directory management |
| `Services/TempBatchStorage.cs` | Implementation with TTL cleanup, safe ID validation |

### Key Features Implemented
1. **50 File Limit** - Up from 10
2. **500MB Total Batch Cap** - Prevents memory exhaustion
3. **30-Minute TTL** - Auto-cleanup of expired batches
4. **Manual Delete Button** - Users can delete their batch immediately
5. **Filename Patterns** - `{original}`, `{name}`, `{date}`, `{time}`, `{width}`, `{height}`
6. **Streaming Download** - Files stored on disk, zipped on-demand

---

## Remaining Tasks

- [x] **Fix ZIP already exists bug** (see above)
- [ ] **Add unit tests** for `TempBatchStorage` (optional)
- [x] **Consider caching the ZIP** - Cached ZIP stored as `_batch.zip` inside batch folder

---

## Files Modified

| File | Changes |
|------|---------|
| `Controllers/ImageToolsController.cs` | Added `ITempBatchStorage` DI, batch limits, delete endpoint |
| `Views/ImageTools/AdvancedResizer.cshtml` | UI for 50 files, delete button, filename pattern help text |
| `Services/ITempBatchStorage.cs` | **NEW** - Interface |
| `Services/TempBatchStorage.cs` | **NEW** - Implementation |
| `Program.cs` | Registered `TempBatchStorage` as singleton |
