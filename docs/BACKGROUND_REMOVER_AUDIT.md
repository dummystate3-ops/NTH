# NovaToolsHub - Background Remover Audit Report (Re-Audit)

Date: 2025-12-19  
Scope: `Services/BackgroundRemovalService.cs`, `Services/TempResultStorage.cs`, `Controllers/ImageToolsController.cs`, `Views/ImageTools/BackgroundRemover.cshtml`, `Program.cs`, `appsettings.json`, model assets

## 1) Executive Summary

The Background Remover has improved significantly since the last audit:
- The strict-mode `event` crash is fixed.
- The image-load race is fixed with a `Promise.all` load path.
- Slider handle alignment is corrected.
- Error UI and keyboard accessibility were added.
- Letterbox preprocessing, request cancellation, and concurrency limiting were added.
- Download flow now uses disk-based temp storage instead of session blobs.

However, there are still two production blockers and several quality/security gaps. Most notably, the model path now points to `App_Data/models/u2net.onnx`, but the actual model file remains in `wwwroot/models/u2net.onnx`. This mismatch will cause the tool to fail at runtime unless the model is moved or the configuration is updated.

## 2) Status of Previous Recommendations

### Resolved
- Event handler crash fixed by passing `event` explicitly to `setEditorBg`.  
  `Views/ImageTools/BackgroundRemover.cshtml`
- Image-load race fixed by `loadImage` + `Promise.all`.  
  `Views/ImageTools/BackgroundRemover.cshtml`
- Slider handle initial alignment fixed.  
  `Views/ImageTools/BackgroundRemover.cshtml`
- Inline error banner with `role="alert"` added; `alert()` removed.  
  `Views/ImageTools/BackgroundRemover.cshtml`
- Keyboard activation on upload zone added.  
  `Views/ImageTools/BackgroundRemover.cshtml`
- Letterbox preprocessing added to avoid aspect ratio distortion.  
  `Services/BackgroundRemovalService.cs`
- Concurrency limiting + cancellation token support added.  
  `Services/BackgroundRemovalService.cs`
- Model path removed from UI messaging (no longer displayed to users).  
  `Views/ImageTools/BackgroundRemover.cshtml`
- Download now streams from disk-based temp storage, not session.  
  `Controllers/ImageToolsController.cs`, `Services/TempResultStorage.cs`

### Partially Addressed
- Base64 session storage removed, but base64 image data is still returned in JSON for preview.  
  `Controllers/ImageToolsController.cs`
- Model moved to configurable path, but the actual file has not been moved (see Critical Findings).  
  `appsettings.json`, `wwwroot/models/u2net.onnx`

### Not Addressed
- ONNX output selection still uses `results.First()` with no output-name or shape validation.  
  `Services/BackgroundRemovalService.cs`
- Mask postprocessing still uses min/max normalization instead of sigmoid/clamp.  
  `Services/BackgroundRemovalService.cs`

## 3) Current Findings (Bugs / Missing Components / Risks)

Severity levels: **Critical**, **High**, **Medium**, **Low**.

### Critical

1) Model path mismatch causes runtime failure  
The configured model path is `App_Data/models/u2net.onnx`, but the repository only contains `wwwroot/models/u2net.onnx`. With the current config, `IsModelLoaded` is false and inference will throw.  
Files: `appsettings.json`, `Services/BackgroundRemovalSettings.cs`, `Services/BackgroundRemovalService.cs`, `wwwroot/models/u2net.onnx`

2) Model is still publicly accessible under `wwwroot/models`  
Static files are enabled and the ONNX file is still in `wwwroot/models`, so it can be downloaded directly if the file remains there.  
Files: `Program.cs`, `wwwroot/models/u2net.onnx`

### High

1) ONNX output selection is still ambiguous  
`results.First()` is used without validating the output name or shape. U2Net models often expose multiple outputs; the wrong tensor can be selected silently.  
File: `Services/BackgroundRemovalService.cs`

2) Base64 preview is still returned in JSON  
Although disk storage now backs downloads, the API still returns the full base64 image in JSON for every request. This is heavy and can exceed response limits for larger images.  
File: `Controllers/ImageToolsController.cs`

3) No max pixel count guard (decompression risk)  
Validation checks only dimensions and file size. A small file can decompress to massive pixel counts, causing memory pressure.  
Files: `Controllers/ImageToolsController.cs`, `Services/BackgroundRemovalService.cs`

### Medium

1) Mask postprocessing likely degrades consistency  
Min/max normalization varies per image. For U2Net, sigmoid or clamp is typically more stable.  
File: `Services/BackgroundRemovalService.cs`

2) Background removal endpoint does not validate content type  
`RemoveBackgroundAjax` checks extension but does not check `ContentType`. Other endpoints do.  
File: `Controllers/ImageToolsController.cs`

3) Editor export resolution is capped without disclosure  
Canvas is capped at `MAX_CANVAS_SIZE` and export uses that downscaled canvas. Users may expect full-res output.  
File: `Views/ImageTools/BackgroundRemover.cshtml`

4) Temp result cleanup is time-based only  
Files are deleted by TTL timer but not removed after download; disk usage can spike under heavy usage.  
File: `Services/TempResultStorage.cs`

5) Error handling assumes JSON response  
`fetch` calls `res.json()` even on non-OK responses; server-side errors can throw confusing parse errors.  
File: `Views/ImageTools/BackgroundRemover.cshtml`

6) Mojibake text still present in UI  
There are visible garbled characters in multiple UI strings, which looks unprofessional.  
File: `Views/ImageTools/BackgroundRemover.cshtml`

### Low

1) Inline `onclick` handlers remain  
Not a bug, but harder to maintain and test than delegated listeners.  
File: `Views/ImageTools/BackgroundRemover.cshtml`

2) Unused `ViewBag.ModelPath` assignment  
The controller still sets `ViewBag.ModelPath`, but the view no longer uses it.  
File: `Controllers/ImageToolsController.cs`

## 4) Recommendations (Prioritized)

### Phase 1 - Fix production blockers
1. Move the ONNX model to `App_Data/models/u2net.onnx` or update `BackgroundRemoval:ModelPath` to match the real file location.  
2. Remove or block public access to `wwwroot/models/u2net.onnx` if it remains there.

### Phase 2 - Quality and scalability
1. Select ONNX output by name or shape (validate `[1,1,320,320]`), and fail fast if unexpected.  
2. Replace base64 preview with a short-lived preview URL served as `image/png`.  
3. Add a max pixel count (e.g., 20-40 MP) in `TryValidateImageDimensions` to prevent decompression bombs.  
4. Use sigmoid or clamp for mask output, optionally add a threshold/feather control.  
5. Add content-type validation for background removal uploads.  
6. Either export full-res images (server-side render) or show a clear "export is scaled" notice.

### Phase 3 - Operational polish
1. Delete temp results after download or add a max disk usage cap.  
2. Add explicit error handling for non-OK responses in the frontend.  
3. Replace garbled UI characters with SVG icons or plain text.  
4. Add integration tests for `RemoveBackgroundAjax` and download flow.

## 5) Architecture (Current, Updated)

1. Client uploads file to `POST /ImageTools/RemoveBackgroundAjax`.  
2. Controller validates file size/extension/dimensions and calls `RemoveBackgroundAsync`.  
3. Service letterboxes, runs ONNX, crops mask, applies alpha, and returns PNG bytes.  
4. PNG is stored in disk temp storage; controller returns base64 preview + download key.  
5. Client uses download key to stream `image/png` from disk via `GET /ImageTools/DownloadBgRemoved`.

## 6) Test Plan (Recommended)

Backend:
- Missing file => `FILE_007`
- Oversize file => `FILE_001`
- Invalid content type => `FILE_002`
- Invalid image bytes => `FILE_003`
- Model missing => `SRV_001`
- Happy path => `success`, download key, and valid PNG stream

Frontend:
- Swatch selection does not throw; active state updates
- Compare slider starts aligned at 50%
- Large images do not freeze tab (canvas cap)
- Error banner shows for server or network failures

## 7) File Reference Index

- `Services/BackgroundRemovalService.cs`
- `Services/BackgroundRemovalSettings.cs`
- `Services/TempResultStorage.cs`
- `Controllers/ImageToolsController.cs`
- `Views/ImageTools/BackgroundRemover.cshtml`
- `Program.cs`
- `appsettings.json`
- `wwwroot/models/u2net.onnx`

## 8) How-To Instructions for a Coding Agent (Non-Hallucination Workflow)

Goal: Apply fixes without guessing. Only act on facts verified in the repo.

### 8.1 Guardrails (do these every time)

1. Confirm files and exact locations with `rg` before modifying anything.  
2. Read the full current file content for any file you will edit.  
3. Do not assume model location. Verify `appsettings.json` and actual model file path.  
4. If a change depends on runtime behavior, either run a targeted test or mark it as "untested."  
5. Do not introduce new files or dependencies unless the report explicitly calls for it.  

### 8.2 Verification Steps (pre-change)

Run these commands to establish ground truth:

```powershell
cd d:\MyProjects\NovaToolsHub
rg -n "BackgroundRemover|RemoveBackgroundAjax|DownloadBgRemoved|BackgroundRemovalService" -S Services Controllers Views Program.cs
Get-Content Services\BackgroundRemovalService.cs -Raw
Get-Content Controllers\ImageToolsController.cs -Raw
Get-Content Views\ImageTools\BackgroundRemover.cshtml -Raw
Get-Content appsettings.json -Raw
Get-ChildItem -Path App_Data\models -ErrorAction SilentlyContinue
Get-ChildItem -Path wwwroot\models -ErrorAction SilentlyContinue
```

If any file is missing or differs from this report, stop and update the audit first.

### 8.3 Task-Specific How-To (apply in order)

1) Fix model path mismatch (critical)  
   - Confirm where `u2net.onnx` actually exists.  
   - Either move the file into `App_Data\models\u2net.onnx` or update `BackgroundRemoval:ModelPath` to point at the real location.  
   - Re-check `IsModelLoaded` behavior in `BackgroundRemovalService`.

2) Remove public access to the model (critical)  
   - If the model remains under `wwwroot`, remove it or block static serving for that directory.  
   - Verify `wwwroot\models` no longer contains the ONNX file.

3) Fix ONNX output selection (high)  
   - Inspect output names in the ONNX metadata (locally) and choose the correct mask output by name.  
   - Add validation: if the expected output is missing or has unexpected shape, log and fail with a clear error.

4) Replace base64 JSON preview (high)  
   - Return a preview URL or reuse `DownloadBgRemoved` with a `?inline=true` style flag.  
   - Update the UI to consume the URL instead of `data:` base64.

5) Add a max pixel-count guard (high)  
   - Add a `MaxImagePixels` constant and check `width * height` in `TryValidateImageDimensions`.  
   - Return `FILE_006` or a new error code if the pixel count exceeds the cap.

6) Improve mask postprocessing (medium)  
   - Replace min/max normalization with sigmoid or clamp.  
   - If using sigmoid, verify output tensor is logits (not already normalized).

7) Editor export clarity (medium)  
   - If the canvas is capped, show a visible note that exports are scaled.  
   - Or implement a full-res export path.

### 8.4 Evidence Checklist (post-change)

For each change, include evidence in the final response:
- File path(s) modified.
- A short description of what changed and why.
- What you verified (command output summary or test run).
- Any remaining risks or TODOs if not fully verified.

### 8.5 Anti-Hallucination Rules (must follow)

- Do not claim a file, setting, or test exists unless you verified it with a command.  
- Do not invent model output names or shapes; inspect them locally.  
- Do not assume a fix is complete until you verify its runtime path.  
- If you cannot verify something (e.g., no model file in expected path), state it clearly and stop.
