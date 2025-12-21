using Microsoft.AspNetCore.Mvc;
using NovaToolsHub.Models.ViewModels;
using NovaToolsHub.Services;
using SixLabors.ImageSharp;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NovaToolsHub.Controllers;

/// <summary>
/// Controller for image processing tools including resizing, compression, and editing
/// </summary>
public class ImageToolsController : Controller
{
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IBackgroundRemovalService _backgroundRemovalService;
    private readonly ITempResultStorage _tempResultStorage;
    private readonly ILogger<ImageToolsController> _logger;
    private readonly ITempBatchStorage _tempBatchStorage;

    // Configuration constants
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
    private const long MaxBatchTotalBytes = 500 * 1024 * 1024; // 500MB per batch cap
    private const int MaxBatchFiles = 50; // Maximum files in batch mode
    private const int MaxImageDimension = 10000;
    private static readonly TimeSpan BatchTtl = TimeSpan.FromMinutes(30);
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp" };
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp", "image/gif", "image/bmp" };

    private static class ErrorCodes
    {
        public const string FileTooLarge = "FILE_001";
        public const string InvalidFormat = "FILE_002";
        public const string InvalidImage = "FILE_003";
        public const string BatchSizeLimit = "FILE_004";
        public const string NoValidFiles = "FILE_005";
        public const string ImageTooLarge = "FILE_006";
        public const string FileMissing = "FILE_007";
        public const string ProcessingFailed = "PROC_002";
        public const string ServerError = "SRV_001";
        public const string BatchIdInvalid = "SRV_002";
        public const string DeleteFailed = "SRV_003";
    }

    public ImageToolsController(
        IImageProcessingService imageProcessingService,
        IBackgroundRemovalService backgroundRemovalService,
        ITempResultStorage tempResultStorage,
        ILogger<ImageToolsController> logger,
        ITempBatchStorage tempBatchStorage)
    {
        _imageProcessingService = imageProcessingService;
        _backgroundRemovalService = backgroundRemovalService;
        _tempResultStorage = tempResultStorage;
        _logger = logger;
        _tempBatchStorage = tempBatchStorage;
    }

    /// <summary>
    /// GET: Display the Advance Image Tool page
    /// </summary>
    [HttpGet]
    [Route("Tools/Image/AdvancedResizer")]
    public IActionResult AdvancedResizer()
    {
        var model = new AdvancedImageResizerViewModel();
        return View(model);
    }

    /// <summary>
    /// GET: Simple Image Compressor tool (client-side only)
    /// </summary>
    [HttpGet]
    [Route("Tools/Image/Compressor")]
    public IActionResult Compressor()
    {
        return View();
    }

    /// <summary>
    /// GET: Display the Background Remover tool page
    /// </summary>
    [HttpGet]
    [Route("Tools/Image/BackgroundRemover")]
    [Route("ImageTools/BackgroundRemover")]
    public IActionResult BackgroundRemover()
    {
        ViewBag.IsGeneralModelLoaded = _backgroundRemovalService.IsGeneralModelLoaded;
        ViewBag.IsPortraitModelLoaded = _backgroundRemovalService.IsPortraitModelLoaded;
        // ViewBag.ModelPath removed - no longer displayed in UI (security)
        return View();
    }

    /// <summary>
    /// POST: Remove background from uploaded image via AJAX
    /// </summary>
    [HttpPost]
    [Route("ImageTools/RemoveBackgroundAjax")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveBackgroundAjax(IFormFile file, string? mode = null, int? portraitEdgeStrength = null)
    {
        if (file == null || file.Length == 0)
        {
            return JsonError(ErrorCodes.FileMissing, "Please select an image file.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return JsonError(ErrorCodes.FileTooLarge, $"File size cannot exceed {MaxFileSizeBytes / (1024 * 1024)}MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return JsonError(ErrorCodes.InvalidFormat, "Invalid file type.");
        }

        // Content-type validation (audit fix)
        if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return JsonError(ErrorCodes.InvalidFormat, "Invalid content type.");
        }

        if (!TryValidateImageDimensions(file, out var dimensionError))
        {
            return JsonError(ErrorCodes.InvalidImage, dimensionError ?? "Invalid image.");
        }

        var parsedMode = BackgroundRemovalMode.General;
        if (!string.IsNullOrWhiteSpace(mode) &&
            Enum.TryParse<BackgroundRemovalMode>(mode, true, out var parsed))
        {
            parsedMode = parsed;
        }

        var edgeStrength = portraitEdgeStrength.HasValue
            ? Math.Clamp(portraitEdgeStrength.Value, 0, 100)
            : (int?)null;

        if (parsedMode == BackgroundRemovalMode.Portrait && !_backgroundRemovalService.IsPortraitModelLoaded)
        {
            return JsonError(ErrorCodes.ServerError, "Portrait model not installed. Please contact administrator.");
        }

        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, HttpContext.RequestAborted);
            var imageData = memoryStream.ToArray();

            var result = await _backgroundRemovalService.RemoveBackgroundAsync(
                imageData,
                parsedMode,
                edgeStrength,
                HttpContext.RequestAborted);

            // Store to temp file (used for both preview and download)
            var downloadKey = _tempResultStorage.Store(result, ".png");

            return Json(new
            {
                success = true,
                // Return URL instead of base64 for better performance (audit fix)
                previewUrl = Url.Action("DownloadBgRemoved", "ImageTools", new { key = downloadKey, inline = true }),
                downloadUrl = Url.Action("DownloadBgRemoved", "ImageTools", new { key = downloadKey }),
                downloadKey = downloadKey,
                originalFileName = Path.GetFileNameWithoutExtension(file.FileName)
            });
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "U2Net model not found");
            return JsonError(ErrorCodes.ServerError, "Background removal model not installed. Please contact administrator.");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("pixels"))
        {
            _logger.LogWarning(ex, "Image too large (pixel count)");
            return JsonError(ErrorCodes.ImageTooLarge, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing background");
            return JsonError(ErrorCodes.ProcessingFailed, "Failed to remove background.");
        }
    }

    /// <summary>
    /// GET: Download the background-removed image (streams from temp file)
    /// </summary>
    [HttpGet]
    [Route("ImageTools/DownloadBgRemoved")]
    public IActionResult DownloadBgRemoved(string key, string? filename, bool inline = false)
    {
        if (string.IsNullOrEmpty(key))
        {
            return NotFound("Invalid download key.");
        }

        // Stream from temp file (cleanup relies on TTL service)
        // Reverted delete-on-close to avoid download race conditions
        var stream = _tempResultStorage.RetrieveStream(key);

        if (stream == null)
        {
            return NotFound("Image not found or expired.");
        }

        var downloadFilename = !string.IsNullOrEmpty(filename)
            ? $"{filename}_no_bg.png"
            : $"background_removed_{DateTime.Now:yyyyMMdd_HHmmss}.png";

        // For inline preview, don't set Content-Disposition as attachment
        if (inline)
        {
            return File(stream, "image/png");
        }

        return File(stream, "image/png", downloadFilename);
    }

    /// <summary>
    /// POST: Process a single uploaded image
    /// </summary>
    [HttpPost]
    [Route("Tools/Image/AdvancedResizer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdvancedResizer(AdvancedImageResizerViewModel model)
    {
        // Validate that a file was uploaded
        if (model.UploadedFile == null || model.UploadedFile.Length == 0)
        {
            ModelState.AddModelError(nameof(model.UploadedFile), "Please select an image file to upload.");
            return View(model);
        }

        // Validate file size
        if (model.UploadedFile.Length > MaxFileSizeBytes)
        {
            ModelState.AddModelError(nameof(model.UploadedFile),
                $"File size cannot exceed {MaxFileSizeBytes / (1024 * 1024)}MB. Your file is {model.UploadedFile.Length / (1024.0 * 1024.0):F2}MB.");
            return View(model);
        }

        // Validate file extension
        var fileExtension = Path.GetExtension(model.UploadedFile.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(fileExtension))
        {
            ModelState.AddModelError(nameof(model.UploadedFile),
                $"Invalid file type. Allowed types: {string.Join(", ", AllowedExtensions)}");
            return View(model);
        }

        // Validate content type
        if (!AllowedContentTypes.Contains(model.UploadedFile.ContentType.ToLowerInvariant()))
        {
            ModelState.AddModelError(nameof(model.UploadedFile),
                "Invalid file content type. Please upload a valid image file.");
            return View(model);
        }

        if (!TryValidateImageDimensions(model.UploadedFile, out var dimensionError))
        {
            ModelState.AddModelError(nameof(model.UploadedFile), dimensionError ?? "Invalid image file.");
            return View(model);
        }

        // Validate dimensions if provided
        if (model.TargetWidth.HasValue && model.TargetWidth.Value <= 0)
        {
            ModelState.AddModelError(nameof(model.TargetWidth), "Width must be greater than 0.");
        }

        if (model.TargetHeight.HasValue && model.TargetHeight.Value <= 0)
        {
            ModelState.AddModelError(nameof(model.TargetHeight), "Height must be greater than 0.");
        }

        // Validate resize percentage
        if (model.ResizePercentage.HasValue && (model.ResizePercentage.Value < 10 || model.ResizePercentage.Value > 500))
        {
            ModelState.AddModelError(nameof(model.ResizePercentage), "Resize percentage must be between 10% and 500%.");
        }

        // Validate quality
        if (model.Quality.HasValue && (model.Quality.Value < 0 || model.Quality.Value > 100))
        {
            ModelState.AddModelError(nameof(model.Quality), "Quality must be between 0 and 100.");
        }

        // Validate rotation degrees
        if (model.RotationDegrees.HasValue)
        {
            var validRotations = new[] { 0, 90, 180, 270 };
            if (!validRotations.Contains(model.RotationDegrees.Value))
            {
                ModelState.AddModelError(nameof(model.RotationDegrees), "Rotation must be 0, 90, 180, or 270 degrees.");
            }
        }

        // Validate target file size
        if (model.TargetFileSizeKb.HasValue && model.TargetFileSizeKb.Value <= 0)
        {
            ModelState.AddModelError(nameof(model.TargetFileSizeKb), "Target file size must be greater than 0 KB.");
        }

        // Validate brightness
        if (model.Brightness.HasValue && (model.Brightness.Value < 0.1f || model.Brightness.Value > 3.0f))
        {
            ModelState.AddModelError(nameof(model.Brightness), "Brightness must be between 0.1 and 3.0.");
        }

        // Validate contrast
        if (model.Contrast.HasValue && (model.Contrast.Value < 0.1f || model.Contrast.Value > 3.0f))
        {
            ModelState.AddModelError(nameof(model.Contrast), "Contrast must be between 0.1 and 3.0.");
        }

        // Validate text watermark options
        if (model.WatermarkFontSize <= 0 || model.WatermarkFontSize > 200)
        {
            ModelState.AddModelError(nameof(model.WatermarkFontSize), "Font size must be between 1 and 200.");
        }

        if (!string.IsNullOrWhiteSpace(model.WatermarkColor) && !IsValidHexColor(model.WatermarkColor))
        {
            ModelState.AddModelError(nameof(model.WatermarkColor), "Invalid color. Use a hex color like #FFFFFF.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            ProcessedImageResult? result = null;

            // Process the image using the uploaded file stream
            // The stream is automatically disposed after the using block
            using (var inputStream = model.UploadedFile.OpenReadStream())
            {
                result = await _imageProcessingService.ProcessSingleAsync(model, inputStream);
            }

            // Store the result in TempData for display and download
            // Convert to base64 for display in the view
            var base64Image = Convert.ToBase64String(result.Data);
            ViewBag.ProcessedImageData = base64Image;
            ViewBag.ProcessedImageMimeType = result.MimeType;
            ViewBag.ProcessingResult = result;

            // Store in TempData for download action
            TempData["ProcessedImageData"] = result.Data;
            TempData["ProcessedImageMimeType"] = result.MimeType;
            TempData["ProcessedImageFormat"] = result.Format;

            ViewBag.SuccessMessage = "Image processed successfully!";

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid image processing operation");
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image");
            ModelState.AddModelError(string.Empty, "An unexpected error occurred while processing your image. Please try again.");
            return View(model);
        }
    }

    /// <summary>
    /// POST: Process image via AJAX and return JSON result
    /// </summary>
    [HttpPost]
    [Route("Tools/Image/ProcessAjax")]
    [Route("ImageTools/ProcessAjax")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessAjax(AdvancedImageResizerViewModel model)
    {
        // Validate that a file was uploaded
        if (model.UploadedFile == null || model.UploadedFile.Length == 0)
        {
            return JsonError(ErrorCodes.FileMissing, "Please select an image file to upload.");
        }

        // Validate file size
        if (model.UploadedFile.Length > MaxFileSizeBytes)
        {
            return JsonError(ErrorCodes.FileTooLarge, $"File size cannot exceed {MaxFileSizeBytes / (1024 * 1024)}MB.");
        }

        // Validate file extension
        var fileExtension = Path.GetExtension(model.UploadedFile.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(fileExtension))
        {
            return JsonError(ErrorCodes.InvalidFormat, $"Invalid file type. Allowed types: {string.Join(", ", AllowedExtensions)}");
        }

        // Validate content type
        if (!AllowedContentTypes.Contains(model.UploadedFile.ContentType.ToLowerInvariant()))
        {
            return JsonError(ErrorCodes.InvalidFormat, "Invalid file content type.");
        }

        if (!TryValidateImageDimensions(model.UploadedFile, out var dimensionError))
        {
            var code = dimensionError != null && dimensionError.Contains("exceed", StringComparison.OrdinalIgnoreCase)
                ? ErrorCodes.ImageTooLarge
                : ErrorCodes.InvalidImage;
            return JsonError(code, dimensionError ?? "Invalid image file.");
        }

        // Validate dimensions if provided
        if (model.TargetWidth.HasValue && model.TargetWidth.Value <= 0)
        {
            return JsonError(ErrorCodes.ProcessingFailed, "Width must be greater than 0.");
        }

        if (model.TargetHeight.HasValue && model.TargetHeight.Value <= 0)
        {
            return JsonError(ErrorCodes.ProcessingFailed, "Height must be greater than 0.");
        }

        // Validate resize percentage
        if (model.ResizePercentage.HasValue && (model.ResizePercentage.Value < 10 || model.ResizePercentage.Value > 500))
        {
            return JsonError(ErrorCodes.ProcessingFailed, "Resize percentage must be between 10% and 500%.");
        }

        // Validate quality
        if (model.Quality.HasValue && (model.Quality.Value < 0 || model.Quality.Value > 100))
        {
            return JsonError(ErrorCodes.ProcessingFailed, "Quality must be between 0 and 100.");
        }

        // Validate brightness
        if (model.Brightness.HasValue && (model.Brightness.Value < 0.1f || model.Brightness.Value > 3.0f))
        {
            return JsonError(ErrorCodes.ProcessingFailed, "Brightness must be between 0.1 and 3.0.");
        }

        // Validate contrast
        if (model.Contrast.HasValue && (model.Contrast.Value < 0.1f || model.Contrast.Value > 3.0f))
        {
            return JsonError(ErrorCodes.ProcessingFailed, "Contrast must be between 0.1 and 3.0.");
        }

        // Validate rotation degrees
        if (model.RotationDegrees.HasValue)
        {
            var validRotations = new[] { 0, 90, 180, 270 };
            if (!validRotations.Contains(model.RotationDegrees.Value))
            {
                return JsonError(ErrorCodes.ProcessingFailed, "Rotation must be 0, 90, 180, or 270 degrees.");
            }
        }

        // Validate text watermark options
        if (model.WatermarkFontSize <= 0 || model.WatermarkFontSize > 200)
        {
            return JsonError(ErrorCodes.ProcessingFailed, "Font size must be between 1 and 200.");
        }

        if (!string.IsNullOrWhiteSpace(model.WatermarkColor) && !IsValidHexColor(model.WatermarkColor))
        {
            return JsonError(ErrorCodes.ProcessingFailed, "Invalid watermark color. Use a hex color like #FFFFFF.");
        }

        try
        {
            ProcessedImageResult result;

            using (var inputStream = model.UploadedFile.OpenReadStream())
            {
                result = await _imageProcessingService.ProcessSingleAsync(model, inputStream);
            }

            // Convert to base64 for client-side display
            var base64Image = Convert.ToBase64String(result.Data);

            // Store in session for download (using a unique key)
            var downloadKey = Guid.NewGuid().ToString();
            HttpContext.Session.SetString($"ProcessedImage_{downloadKey}_Data", Convert.ToBase64String(result.Data));
            HttpContext.Session.SetString($"ProcessedImage_{downloadKey}_MimeType", result.MimeType);
            HttpContext.Session.SetString($"ProcessedImage_{downloadKey}_Format", result.Format);

            return Json(new
            {
                success = true,
                imageData = base64Image,
                mimeType = result.MimeType,
                originalWidth = result.OriginalWidth,
                originalHeight = result.OriginalHeight,
                originalSize = result.GetOriginalSizeFormatted(),
                processedWidth = result.ProcessedWidth,
                processedHeight = result.ProcessedHeight,
                processedSize = result.GetProcessedSizeFormatted(),
                compressionRatio = result.GetCompressionRatio(),
                format = result.Format,
                downloadKey = downloadKey
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid image processing operation");
            return JsonError(ErrorCodes.ProcessingFailed, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image");
            return JsonError(ErrorCodes.ServerError, "An unexpected error occurred while processing your image.");
        }
    }

    /// <summary>
    /// POST: Generate a fast preview of image with effects applied (for live preview)
    /// </summary>
    [HttpPost]
    [Route("ImageTools/GeneratePreview")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GeneratePreview(AdvancedImageResizerViewModel model)
    {
        if (model.UploadedFile == null || model.UploadedFile.Length == 0)
        {
            return JsonError(ErrorCodes.FileMissing, "No image provided.");
        }

        // Quick validation
        if (model.UploadedFile.Length > MaxFileSizeBytes)
        {
            return JsonError(ErrorCodes.FileTooLarge, "File too large.");
        }

        if (!TryValidateImageDimensions(model.UploadedFile, out var dimensionError))
        {
            var code = dimensionError != null && dimensionError.Contains("exceed", StringComparison.OrdinalIgnoreCase)
                ? ErrorCodes.ImageTooLarge
                : ErrorCodes.InvalidImage;
            return JsonError(code, dimensionError ?? "Invalid image file.");
        }

        try
        {
            ProcessedImageResult result;
            using (var inputStream = model.UploadedFile.OpenReadStream())
            {
                result = await _imageProcessingService.GeneratePreviewAsync(model, inputStream);
            }

            var base64Image = Convert.ToBase64String(result.Data);

            return Json(new
            {
                success = true,
                imageData = base64Image,
                mimeType = result.MimeType,
                width = result.ProcessedWidth,
                height = result.ProcessedHeight
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating preview");
            return JsonError(ErrorCodes.ServerError, "Failed to generate preview.");
        }
    }

    /// <summary>
    /// Download the processed image
    /// </summary>
    [HttpGet]
    [Route("Tools/Image/Download")]
    [Route("ImageTools/Download")]
    public IActionResult DownloadProcessedImage(string? key)
    {
        byte[]? imageData = null;
        string? mimeType = null;
        string? format = null;
        bool inline = string.Equals(HttpContext.Request.Query["inline"], "true", StringComparison.OrdinalIgnoreCase);

        // Try to get from session first (AJAX flow)
        if (!string.IsNullOrEmpty(key))
        {
            var base64Data = HttpContext.Session.GetString($"ProcessedImage_{key}_Data");
            mimeType = HttpContext.Session.GetString($"ProcessedImage_{key}_MimeType");
            format = HttpContext.Session.GetString($"ProcessedImage_{key}_Format");

            if (!string.IsNullOrEmpty(base64Data))
            {
                imageData = Convert.FromBase64String(base64Data);
            }
        }

        // Fallback to TempData (non-AJAX flow)
        if (imageData == null)
        {
            imageData = TempData["ProcessedImageData"] as byte[];
            mimeType = TempData["ProcessedImageMimeType"] as string;
            format = TempData["ProcessedImageFormat"] as string;
        }

        if (imageData == null || string.IsNullOrEmpty(mimeType) || string.IsNullOrEmpty(format))
        {
            return NotFound("No processed image found. Please process an image first.");
        }

        // Generate a filename with timestamp
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"processed_image_{timestamp}.{format}";

        return inline ? File(imageData, mimeType) : File(imageData, mimeType, fileName);
    }

    /// <summary>
    /// Process multiple images in batch mode via AJAX
    /// </summary>
    [HttpPost]
    [Route("Tools/Image/ProcessBatch")]
    [Route("ImageTools/ProcessBatch")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessBatch([FromForm] AdvancedImageResizerViewModel model, [FromForm] List<IFormFile> files)
    {
        _tempBatchStorage.CleanupExpiredBatches(BatchTtl);

        // Validate batch size
        if (files == null || files.Count == 0)
        {
            return JsonError(ErrorCodes.FileMissing, "Please select at least one image file.");
        }

        if (files.Count > MaxBatchFiles)
        {
            return JsonError(ErrorCodes.BatchSizeLimit, $"Maximum {MaxBatchFiles} files allowed in batch mode.");
        }

        var totalBytes = files.Sum(f => f.Length);
        if (totalBytes > MaxBatchTotalBytes)
        {
            return JsonError(ErrorCodes.BatchSizeLimit, $"Total batch size exceeds {MaxBatchTotalBytes / (1024 * 1024)}MB. Please upload smaller or fewer files.");
        }

        // Validate each file
        var streams = new List<Stream>();
        var fileNames = new List<string>();

        try
        {
            foreach (var file in files)
            {
                if (file.Length > MaxFileSizeBytes)
                {
                    return JsonError(ErrorCodes.FileTooLarge, $"File '{file.FileName}' exceeds maximum size of {MaxFileSizeBytes / (1024 * 1024)}MB.");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    return JsonError(ErrorCodes.InvalidFormat, $"File '{file.FileName}' has an invalid extension.");
                }

                if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                {
                    return JsonError(ErrorCodes.InvalidFormat, $"File '{file.FileName}' has an invalid content type.");
                }

                if (!TryValidateImageDimensions(file, out var dimensionError))
                {
                    var message = dimensionError ?? "Invalid image file.";
                    var code = message.Contains("exceed", StringComparison.OrdinalIgnoreCase)
                        ? ErrorCodes.ImageTooLarge
                        : ErrorCodes.InvalidImage;
                    return JsonError(code, $"File '{file.FileName}': {message}");
                }

                streams.Add(file.OpenReadStream());
                fileNames.Add(file.FileName); // Use full filename with extension for pattern logic
            }

            // Create Temp Directory
            var batchId = Guid.NewGuid().ToString("N");
            var tempDir = _tempBatchStorage.CreateBatchDirectory(batchId);

            // Process batch and save to disk
            var savedFiles = await _imageProcessingService.ProcessBatchAndSaveAsync(model, streams, fileNames, tempDir);

            // Check if any images were actually processed
            if (savedFiles.Count == 0)
            {
                _tempBatchStorage.DeleteBatch(batchId);
                return JsonError(ErrorCodes.NoValidFiles, "No images could be processed.");
            }

            var sampleNames = savedFiles.Take(8).Select(Path.GetFileName).ToList();

            return Json(new
            {
                success = true,
                batchId = batchId,
                processedCount = savedFiles.Count,
                sampleFileNames = sampleNames,
                message = $"{savedFiles.Count} images ready for download."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch images");
            return JsonError(ErrorCodes.ServerError, "An error occurred while processing your images.");
        }
        finally
        {
            foreach (var stream in streams)
            {
                stream?.Dispose();
            }
        }
    }

    private bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color)) return false;

        color = color.Trim();
        if (!color.StartsWith("#")) return false;

        var hex = color[1..];
        return (hex.Length == 3 || hex.Length == 6) && hex.All(c => "0123456789abcdefABCDEF".Contains(c));
    }

    private IActionResult JsonError(string code, string message)
    {
        return Json(new { success = false, error = message, errorCode = code });
    }

    private bool TryValidateImageDimensions(IFormFile file, out string? errorMessage)
    {
        errorMessage = null;

        try
        {
            using var stream = file.OpenReadStream();
            var info = Image.Identify(stream);
            if (info == null)
            {
                errorMessage = "Invalid image file.";
                return false;
            }

            if (info.Width > MaxImageDimension || info.Height > MaxImageDimension)
            {
                errorMessage = $"Image dimensions ({info.Width}x{info.Height}) exceed maximum allowed.";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read image dimensions for {FileName}", file.FileName);
            errorMessage = "Invalid image file.";
            return false;
        }
    }

    [HttpGet]
    [Route("Tools/Image/DownloadBatch")]
    [Route("ImageTools/DownloadBatch")]
    public IActionResult DownloadBatch(string batchId)
    {
        _tempBatchStorage.CleanupExpiredBatches(BatchTtl);

        if (string.IsNullOrEmpty(batchId)) return BadRequest("Invalid batch ID.");

        // Sanitized ID to prevent directory traversal
        if (!_tempBatchStorage.IsSafeBatchId(batchId)) return BadRequest("Invalid batch ID.");

        var tempDir = _tempBatchStorage.GetBatchDirectory(batchId);
        if (!Directory.Exists(tempDir))
        {
            return NotFound("Batch expired or not found.");
        }

        try
        {
            // Store ZIP alongside batch folder for caching
            var cachedZip = Path.Combine(tempDir, "_batch.zip");

            // Check if cached ZIP exists and is valid
            if (!System.IO.File.Exists(cachedZip))
            {
                // Create ZIP from directory (exclude the zip file itself)
                var tempZip = Path.Combine(Path.GetTempPath(), $"nova_batch_{batchId}_temp.zip");

                // Clean up any stale temp zip
                if (System.IO.File.Exists(tempZip))
                {
                    try { System.IO.File.Delete(tempZip); } catch { /* ignore */ }
                }

                ZipFile.CreateFromDirectory(tempDir, tempZip);

                // Move to cached location
                System.IO.File.Move(tempZip, cachedZip);
            }

            // Open cached ZIP for reading (don't delete on close - it's the cache)
            var fs = new FileStream(cachedZip, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);

            return File(fs, "application/zip", $"processed_images_{DateTime.Now:yyyyMMdd_HHmm}.zip");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating batch download");
            return StatusCode(500, "Error generating download.");
        }
    }

    [HttpPost]
    [Route("Tools/Image/DeleteBatch")]
    [Route("ImageTools/DeleteBatch")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteBatch(string batchId)
    {
        _tempBatchStorage.CleanupExpiredBatches(BatchTtl);

        if (string.IsNullOrWhiteSpace(batchId))
        {
            return JsonError(ErrorCodes.BatchIdInvalid, "Invalid batch ID.");
        }

        if (!_tempBatchStorage.IsSafeBatchId(batchId))
        {
            return JsonError(ErrorCodes.BatchIdInvalid, "Invalid batch ID.");
        }

        if (!_tempBatchStorage.BatchExists(batchId))
        {
            return JsonError(ErrorCodes.BatchIdInvalid, "Batch not found or already deleted.");
        }

        try
        {
            var (filesDeleted, bytesFreed) = _tempBatchStorage.DeleteBatch(batchId);
            var bytesFreedFormatted = bytesFreed >= 1024 * 1024
                ? $"{bytesFreed / (1024.0 * 1024.0):F1} MB"
                : $"{bytesFreed / 1024.0:F1} KB";
            return Json(new { success = true, filesDeleted, bytesFreed, bytesFreedFormatted });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting batch {BatchId}", batchId);
            return JsonError(ErrorCodes.DeleteFailed, "Failed to delete batch files.");
        }
    }
}
