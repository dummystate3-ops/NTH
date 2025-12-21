using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.Fonts;
using NovaToolsHub.Models.ViewModels;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using SixLabors.ImageSharp.PixelFormats;
using SixRectangle = SixLabors.ImageSharp.Rectangle;
using SixPoint = SixLabors.ImageSharp.Point;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingGraphics = System.Drawing.Graphics;
using DrawingSolidBrush = System.Drawing.SolidBrush;
using DrawingColor = System.Drawing.Color;
using DrawingFont = System.Drawing.Font;
using DrawingFontFamily = System.Drawing.FontFamily;
using DrawingPointF = System.Drawing.PointF;
using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using DrawingPixelFormat = System.Drawing.Imaging.PixelFormat;
using DrawingSmoothingMode = System.Drawing.Drawing2D.SmoothingMode;
using DrawingTextRenderingHint = System.Drawing.Text.TextRenderingHint;
using DrawingGraphicsUnit = System.Drawing.GraphicsUnit;
using DrawingFontStyle = System.Drawing.FontStyle;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace NovaToolsHub.Services;

/// <summary>
/// Service for advanced image processing operations
/// </summary>
public class ImageProcessingService : IImageProcessingService
{
    private readonly ILogger<ImageProcessingService> _logger;

    public ImageProcessingService(ILogger<ImageProcessingService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Process a single image with resize, compression, rotation, and effects
    /// </summary>
    public async Task<ProcessedImageResult> ProcessSingleAsync(AdvancedImageResizerViewModel model, Stream inputStream)
    {
        try
        {
            using var image = await ImageSharpImage.LoadAsync(inputStream);
            var originalFormat = image.Metadata.DecodedImageFormat;

            // Store original dimensions and size
            var originalWidth = image.Width;
            var originalHeight = image.Height;
            var originalSize = inputStream.Length;

            // 1. Apply crop first to update working dimensions (only if enabled)
            if (model.ApplyCrop)
            {
                image.Mutate(ctx =>
                {
                    ApplyCrop(ctx, model, image.Width, image.Height);
                });
            }

            var postCropWidth = image.Width;
            var postCropHeight = image.Height;

            // 2. Apply remaining transformations based on post-crop dimensions
            image.Mutate(ctx =>
            {
                // Resize uses post-crop dimensions as the starting point
                if (model.ApplyResize)
                {
                    ApplyResize(ctx, model, postCropWidth, postCropHeight);
                }

                if (model.ApplyEffects)
                {
                    // Apply rotation
                    if (model.RotationDegrees.HasValue && model.RotationDegrees.Value != 0)
                    {
                        ctx.Rotate(model.RotationDegrees.Value);
                    }

                    // Apply flips
                    if (model.FlipHorizontal)
                    {
                        ctx.Flip(FlipMode.Horizontal);
                    }
                    if (model.FlipVertical)
                    {
                        ctx.Flip(FlipMode.Vertical);
                    }

                    // Apply color effects - Grayscale converts to grayscale color space
                    if (model.ApplyGrayscale)
                    {
                        ctx.Grayscale();
                    }

                    // Apply sepia effect - Gives images a warm brownish tone
                    if (model.ApplySepia)
                    {
                        ctx.Sepia();
                    }

                    // Apply brightness adjustment - Values > 1.0 brighten, < 1.0 darken
                    if (model.Brightness.HasValue && Math.Abs(model.Brightness.Value - 1.0f) > 0.01f)
                    {
                        ctx.Brightness(model.Brightness.Value);
                    }

                    // Apply contrast adjustment - Values > 1.0 increase contrast, < 1.0 decrease
                    if (model.Contrast.HasValue && Math.Abs(model.Contrast.Value - 1.0f) > 0.01f)
                    {
                        ctx.Contrast(model.Contrast.Value);
                    }
                }
            });

            // Apply watermark after all other processing (needs to be separate to handle text/image watermarks)
            if (model.ApplyWatermark)
            {
                await ApplyWatermarkAsync(image, model);
            }

            // Handle Metadata: Strip by default for privacy unless explicitly preserved
            if (!model.PreserveMetadata)
            {
                image.Metadata.ExifProfile = null;
                image.Metadata.IptcProfile = null;
                image.Metadata.XmpProfile = null;
            }

            // Determine output format and encoder
            var requestedFormat = model.ApplyCompression ? (model.OutputFormat ?? "original") : "original";
            var quality = model.ApplyCompression ? (model.Quality ?? 90) : 100;
            var (encoder, mimeType, finalFormat) = GetEncoderAndMimeType(requestedFormat, quality, originalFormat);

            // Handle target file size compression if specified
            byte[] processedData;
            if (model.ApplyCompression && model.TargetFileSizeKb.HasValue && model.TargetFileSizeKb.Value > 0)
            {
                processedData = await CompressToTargetSize(image, encoder, model.TargetFileSizeKb.Value, quality);
            }
            else
            {
                using var outputStream = new MemoryStream();
                await image.SaveAsync(outputStream, encoder);
                processedData = outputStream.ToArray();
            }

            return new ProcessedImageResult
            {
                Data = processedData,
                MimeType = mimeType,
                OriginalWidth = originalWidth,
                OriginalHeight = originalHeight,
                OriginalSizeBytes = originalSize,
                ProcessedWidth = image.Width,
                ProcessedHeight = image.Height,
                ProcessedSizeBytes = processedData.Length,
                Format = finalFormat
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image");
            throw new InvalidOperationException("Failed to process image. The file may be corrupted or in an unsupported format.", ex);
        }
    }

    /// <summary>
    /// Apply resize operation based on model parameters
    /// </summary>
    private void ApplyResize(IImageProcessingContext ctx, AdvancedImageResizerViewModel model, int originalWidth, int originalHeight)
    {
        int targetWidth = originalWidth;
        int targetHeight = originalHeight;

        // Check if explicit dimensions are provided - these take priority
        bool hasExplicitDimensions = model.TargetWidth.HasValue || model.TargetHeight.HasValue;

        // Only use percentage if it differs from 100% (default) and no explicit dimensions are set
        bool usePercentageScaling = model.ResizePercentage.HasValue &&
                                    model.ResizePercentage.Value != 100 &&
                                    !hasExplicitDimensions;

        if (usePercentageScaling)
        {
            // Calculate target dimensions based on resize percentage
            var scale = model.ResizePercentage.Value / 100.0;
            targetWidth = (int)(originalWidth * scale);
            targetHeight = (int)(originalHeight * scale);
        }
        else if (hasExplicitDimensions)
        {
            // Use explicit width/height if provided
            if (model.MaintainAspectRatio)
            {
                // Calculate dimensions maintaining aspect ratio
                if (model.TargetWidth.HasValue && model.TargetHeight.HasValue)
                {
                    // Both provided: fit within bounds
                    var widthRatio = (double)model.TargetWidth.Value / originalWidth;
                    var heightRatio = (double)model.TargetHeight.Value / originalHeight;
                    var ratio = Math.Min(widthRatio, heightRatio);

                    targetWidth = (int)(originalWidth * ratio);
                    targetHeight = (int)(originalHeight * ratio);
                }
                else if (model.TargetWidth.HasValue)
                {
                    // Only width provided
                    var ratio = (double)model.TargetWidth.Value / originalWidth;
                    targetWidth = model.TargetWidth.Value;
                    targetHeight = (int)(originalHeight * ratio);
                }
                else if (model.TargetHeight.HasValue)
                {
                    // Only height provided
                    var ratio = (double)model.TargetHeight.Value / originalHeight;
                    targetHeight = model.TargetHeight.Value;
                    targetWidth = (int)(originalWidth * ratio);
                }
            }
            else
            {
                // No aspect ratio maintenance: use exact values or keep original
                targetWidth = model.TargetWidth ?? originalWidth;
                targetHeight = model.TargetHeight ?? originalHeight;
            }
        }

        // Only resize if dimensions have changed
        if (targetWidth != originalWidth || targetHeight != originalHeight)
        {
            ctx.Resize(targetWidth, targetHeight);
        }
    }

    /// <summary>
    /// Get the appropriate encoder and MIME type for the requested output format
    /// </summary>
    private (IImageEncoder encoder, string mimeType, string format) GetEncoderAndMimeType(string format, int quality, IImageFormat? originalFormat)
    {
        // Ensure quality is within valid range
        quality = Math.Clamp(quality, 0, 100);

        var normalized = (format ?? "original").ToLowerInvariant();

        if (normalized == "original" && originalFormat != null)
        {
            if (originalFormat == JpegFormat.Instance)
            {
                return (new JpegEncoder { Quality = quality }, "image/jpeg", "jpg");
            }
            if (originalFormat == PngFormat.Instance)
            {
                return (new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression }, "image/png", "png");
            }
            if (originalFormat == WebpFormat.Instance)
            {
                return (new WebpEncoder { Quality = quality }, "image/webp", "webp");
            }
            if (originalFormat == GifFormat.Instance)
            {
                return (new GifEncoder(), "image/gif", "gif");
            }
        }

        return normalized switch
        {
            "jpg" or "jpeg" => (new JpegEncoder { Quality = quality }, "image/jpeg", "jpg"),
            "png" => (new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression }, "image/png", "png"),
            "webp" => (new WebpEncoder { Quality = quality }, "image/webp", "webp"),
            "gif" => (new GifEncoder(), "image/gif", "gif"),
            _ => (new JpegEncoder { Quality = quality }, "image/jpeg", "jpg") // Default to JPEG
        };
    }

    /// <summary>
    /// Compress image to approximate target file size using iterative quality reduction
    /// Strategy: Binary search approach to find optimal quality that meets target size
    /// </summary>
    private async Task<byte[]> CompressToTargetSize(ImageSharpImage image, IImageEncoder baseEncoder, long targetSizeKb, int initialQuality)
    {
        long targetSizeBytes = targetSizeKb * 1024;
        int minQuality = 10;
        int maxQuality = initialQuality;
        byte[]? bestResult = null;
        long bestSize = long.MaxValue;

        // For formats that don't support quality adjustment, just encode once
        if (baseEncoder is not JpegEncoder and not WebpEncoder)
        {
            using var stream = new MemoryStream();
            await image.SaveAsync(stream, baseEncoder);
            return stream.ToArray();
        }

        // Binary search for optimal quality
        int iterations = 0;
        const int maxIterations = 10;

        while (minQuality <= maxQuality && iterations < maxIterations)
        {
            int currentQuality = (minQuality + maxQuality) / 2;
            iterations++;

            // Create encoder with current quality
            var encoder = baseEncoder switch
            {
                JpegEncoder => new JpegEncoder { Quality = currentQuality },
                WebpEncoder => new WebpEncoder { Quality = currentQuality },
                _ => baseEncoder
            };

            using var testStream = new MemoryStream();
            await image.SaveAsync(testStream, encoder);
            var currentSize = testStream.Length;

            // Update best result if this is closer to target
            if (Math.Abs(currentSize - targetSizeBytes) < Math.Abs(bestSize - targetSizeBytes))
            {
                bestResult = testStream.ToArray();
                bestSize = currentSize;
            }

            // Adjust search range
            if (currentSize > targetSizeBytes)
            {
                maxQuality = currentQuality - 1;
            }
            else if (currentSize < targetSizeBytes)
            {
                minQuality = currentQuality + 1;
            }
            else
            {
                // Exact match found
                break;
            }
        }

        // If we couldn't get close enough with quality alone, return best attempt
        return bestResult ?? await EncodeWithQuality(image, baseEncoder, Math.Max(minQuality, 1));
    }

    /// <summary>
    /// Helper method to encode image with specific quality
    /// </summary>
    private async Task<byte[]> EncodeWithQuality(ImageSharpImage image, IImageEncoder baseEncoder, int quality)
    {
        var encoder = baseEncoder switch
        {
            JpegEncoder => new JpegEncoder { Quality = quality },
            WebpEncoder => new WebpEncoder { Quality = quality },
            _ => baseEncoder
        };

        using var stream = new MemoryStream();
        await image.SaveAsync(stream, encoder);
        return stream.ToArray();
    }

    /// <summary>
    /// Apply crop operation based on preset or custom coordinates
    /// </summary>
    private void ApplyCrop(IImageProcessingContext ctx, AdvancedImageResizerViewModel model, int originalWidth, int originalHeight)
    {
        SixRectangle cropRect;

        if (!string.IsNullOrEmpty(model.CropPreset) && model.CropPreset != "none")
        {
            // Calculate crop rectangle based on preset aspect ratio
            cropRect = model.CropPreset switch
            {
                "1:1" => CalculateCropRectangle(originalWidth, originalHeight, 1, 1),
                "4:3" => CalculateCropRectangle(originalWidth, originalHeight, 4, 3),
                "16:9" => CalculateCropRectangle(originalWidth, originalHeight, 16, 9),
                "3:2" => CalculateCropRectangle(originalWidth, originalHeight, 3, 2),
                _ => new SixRectangle(0, 0, originalWidth, originalHeight)
            };
        }
        else if (model.CropX.HasValue && model.CropY.HasValue && model.CropWidth.HasValue && model.CropHeight.HasValue)
        {
            // Use custom crop coordinates
            var x = Math.Max(0, Math.Min(model.CropX.Value, originalWidth));
            var y = Math.Max(0, Math.Min(model.CropY.Value, originalHeight));
            var width = Math.Min(model.CropWidth.Value, originalWidth - x);
            var height = Math.Min(model.CropHeight.Value, originalHeight - y);

            cropRect = new SixRectangle(x, y, width, height);
        }
        else
        {
            return; // No cropping needed
        }

        if (cropRect.Width > 0 && cropRect.Height > 0)
        {
            ctx.Crop(cropRect);
        }
    }

    /// <summary>
    /// Calculate crop rectangle for a given aspect ratio, centered on the image
    /// </summary>
    private SixRectangle CalculateCropRectangle(int imageWidth, int imageHeight, int ratioWidth, int ratioHeight)
    {
        double targetRatio = (double)ratioWidth / ratioHeight;
        double imageRatio = (double)imageWidth / imageHeight;

        int cropWidth, cropHeight, cropX, cropY;

        if (imageRatio > targetRatio)
        {
            // Image is wider than target ratio - crop width
            cropHeight = imageHeight;
            cropWidth = (int)(imageHeight * targetRatio);
            cropX = (imageWidth - cropWidth) / 2;
            cropY = 0;
        }
        else
        {
            // Image is taller than target ratio - crop height
            cropWidth = imageWidth;
            cropHeight = (int)(imageWidth / targetRatio);
            cropX = 0;
            cropY = (imageHeight - cropHeight) / 2;
        }

        return new SixRectangle(cropX, cropY, cropWidth, cropHeight);
    }

    /// <summary>
    /// Apply watermark (text or image) to the processed image
    /// </summary>
    private async Task ApplyWatermarkAsync(ImageSharpImage image, AdvancedImageResizerViewModel model)
    {
        // Apply text watermark
        if (!string.IsNullOrWhiteSpace(model.WatermarkText))
        {
            try
            {
                using var textWatermark = RenderTextWatermarkImage(
                    model.WatermarkText,
                    model.WatermarkFontFamily,
                    model.WatermarkFontSize,
                    model.WatermarkColor);

                var textPosition = CalculateWatermarkImagePosition(image.Width, image.Height, textWatermark.Width, textWatermark.Height, model.WatermarkPosition);

                image.Mutate(ctx =>
                {
                    ctx.DrawImage(textWatermark, textPosition, model.WatermarkOpacity);
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to apply text watermark, continuing without it");
            }
        }

        // Apply image watermark
        if (model.WatermarkImageFile != null && model.WatermarkImageFile.Length > 0)
        {
            try
            {
                using var watermarkStream = model.WatermarkImageFile.OpenReadStream();
                using var watermark = await ImageSharpImage.LoadAsync(watermarkStream);

                // Resize watermark to 20% of base image width while maintaining aspect ratio
                var maxWatermarkWidth = (int)(image.Width * 0.2);
                if (watermark.Width > maxWatermarkWidth)
                {
                    var ratio = (double)maxWatermarkWidth / watermark.Width;
                    var newHeight = (int)(watermark.Height * ratio);
                    watermark.Mutate(ctx => ctx.Resize(maxWatermarkWidth, newHeight));
                }

                var position = CalculateWatermarkImagePosition(image.Width, image.Height, watermark.Width, watermark.Height, model.WatermarkPosition);

                image.Mutate(ctx =>
                {
                    ctx.DrawImage(watermark, position, model.WatermarkOpacity);
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to apply image watermark, continuing without it");
            }
        }
    }

    /// <summary>
    /// Calculate watermark text position based on position setting (kept for future use)
    /// </summary>
    /// <summary>
    /// Calculate watermark image position
    /// </summary>
    private SixPoint CalculateWatermarkImagePosition(int imageWidth, int imageHeight, int watermarkWidth, int watermarkHeight, string position)
    {
        var padding = 20;

        return position switch
        {
            "TopLeft" => new SixPoint(padding, padding),
            "TopRight" => new SixPoint(imageWidth - watermarkWidth - padding, padding),
            "Center" => new SixPoint((imageWidth - watermarkWidth) / 2, (imageHeight - watermarkHeight) / 2),
            "BottomLeft" => new SixPoint(padding, imageHeight - watermarkHeight - padding),
            "BottomRight" => new SixPoint(imageWidth - watermarkWidth - padding, imageHeight - watermarkHeight - padding),
            _ => new SixPoint(imageWidth - watermarkWidth - padding, imageHeight - watermarkHeight - padding)
        };
    }

    /// <summary>
    /// Render text into an ImageSharp image for watermarking (fallback using System.Drawing).
    /// </summary>
    private Image<Rgba32> RenderTextWatermarkImage(string text, string fontFamily, int fontSize, string colorHex)
    {
        var resolvedFontSize = Math.Clamp(fontSize, 1, 200);
        var resolvedFontFamily = string.IsNullOrWhiteSpace(fontFamily) ? "Arial" : fontFamily;

        DrawingColor resolvedColor;
        try
        {
            resolvedColor = System.Drawing.ColorTranslator.FromHtml(colorHex);
        }
        catch
        {
            resolvedColor = System.Drawing.Color.White;
        }

        using var tempBitmap = new DrawingBitmap(1, 1, DrawingPixelFormat.Format32bppArgb);
        using var tempGraphics = DrawingGraphics.FromImage(tempBitmap);
        using var font = CreateFont(resolvedFontFamily, resolvedFontSize);

        tempGraphics.TextRenderingHint = DrawingTextRenderingHint.AntiAlias;
        var measured = tempGraphics.MeasureString(text, font);
        var width = Math.Max(1, (int)Math.Ceiling(measured.Width));
        var height = Math.Max(1, (int)Math.Ceiling(measured.Height));

        using var bitmap = new DrawingBitmap(width, height, DrawingPixelFormat.Format32bppArgb);
        using (var graphics = DrawingGraphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = DrawingSmoothingMode.AntiAlias;
            graphics.TextRenderingHint = DrawingTextRenderingHint.AntiAlias;
            graphics.Clear(System.Drawing.Color.Transparent);

            using var brush = new DrawingSolidBrush(resolvedColor);
            graphics.DrawString(text, font, brush, new DrawingPointF(0, 0));
        }

        using var stream = new MemoryStream();
        bitmap.Save(stream, DrawingImageFormat.Png);
        stream.Position = 0;
        return ImageSharpImage.Load<Rgba32>(stream.ToArray());
    }

    private DrawingFont CreateFont(string fontFamily, int fontSize)
    {
        try
        {
            return new DrawingFont(new DrawingFontFamily(fontFamily), fontSize, DrawingFontStyle.Bold, DrawingGraphicsUnit.Pixel);
        }
        catch
        {
            return new DrawingFont(DrawingFontFamily.GenericSansSerif, fontSize, DrawingFontStyle.Bold, DrawingGraphicsUnit.Pixel);
        }
    }

    /// <summary>
    /// Process multiple images in batch mode using parallel processing
    /// </summary>
    public async Task<List<ProcessedImageResult>> ProcessBatchAsync(AdvancedImageResizerViewModel model, List<Stream> inputStreams, List<string> fileNames)
    {
        var results = new ConcurrentBag<(int index, ProcessedImageResult result)>();
        var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 4) };

        await Parallel.ForEachAsync(
            inputStreams.Select((stream, index) => (stream, index, fileName: fileNames[index])),
            options,
            async (item, cancellationToken) =>
            {
                try
                {
                    // Copy stream to MemoryStream for thread-safe processing
                    using var memoryStream = new MemoryStream();
                    await item.stream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Position = 0;

                    var result = await ProcessSingleAsync(model, memoryStream);
                    result.FileName = item.fileName;
                    results.Add((item.index, result));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing image {item.fileName} in batch");
                    // Continue processing other images even if one fails
                }
            });

        // Sort by original index to maintain order
        return results.OrderBy(r => r.index).Select(r => r.result).ToList();
    }

    /// <summary>
    /// Generate a fast low-resolution preview with effects applied (no watermark, reduced quality)
    /// </summary>
    public async Task<ProcessedImageResult> GeneratePreviewAsync(AdvancedImageResizerViewModel model, Stream inputStream, int maxDimension = 400)
    {
        try
        {
            using var image = await ImageSharpImage.LoadAsync(inputStream);

            var originalWidth = image.Width;
            var originalHeight = image.Height;

            // Calculate preview dimensions (fit within maxDimension)
            int previewWidth, previewHeight;
            if (image.Width > image.Height)
            {
                previewWidth = Math.Min(image.Width, maxDimension);
                previewHeight = (int)((double)previewWidth / image.Width * image.Height);
            }
            else
            {
                previewHeight = Math.Min(image.Height, maxDimension);
                previewWidth = (int)((double)previewHeight / image.Height * image.Width);
            }

            image.Mutate(ctx =>
            {
                // Always resize for preview
                ctx.Resize(previewWidth, previewHeight);

                // Apply effects preview (skip crop, resize, watermark for speed)
                if (model.ApplyEffects && model.RotationDegrees.HasValue && model.RotationDegrees.Value != 0)
                {
                    ctx.Rotate(model.RotationDegrees.Value);
                }

                if (model.ApplyEffects && model.FlipHorizontal) ctx.Flip(FlipMode.Horizontal);
                if (model.ApplyEffects && model.FlipVertical) ctx.Flip(FlipMode.Vertical);
                if (model.ApplyEffects && model.ApplyGrayscale) ctx.Grayscale();
                if (model.ApplyEffects && model.ApplySepia) ctx.Sepia();

                if (model.ApplyEffects && model.Brightness.HasValue && Math.Abs(model.Brightness.Value - 1.0f) > 0.01f)
                {
                    ctx.Brightness(model.Brightness.Value);
                }

                if (model.ApplyEffects && model.Contrast.HasValue && Math.Abs(model.Contrast.Value - 1.0f) > 0.01f)
                {
                    ctx.Contrast(model.Contrast.Value);
                }
            });

            // Use low quality JPEG for fast preview
            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, new JpegEncoder { Quality = 60 });
            var previewData = outputStream.ToArray();

            return new ProcessedImageResult
            {
                Data = previewData,
                MimeType = "image/jpeg",
                OriginalWidth = originalWidth,
                OriginalHeight = originalHeight,
                ProcessedWidth = image.Width,
                ProcessedHeight = image.Height,
                ProcessedSizeBytes = previewData.Length,
                Format = "jpg"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating preview");
            throw new InvalidOperationException("Failed to generate preview.", ex);
        }
    }
    /// <summary>
    /// Process batch and save directly to disk (memory efficient)
    /// </summary>
    public async Task<List<string>> ProcessBatchAndSaveAsync(AdvancedImageResizerViewModel model, List<Stream> inputStreams, List<string> fileNames, string outputDirectory)
    {
        var savedFilePaths = new System.Collections.Concurrent.ConcurrentBag<string>();
        // Use reasonable concurrency limit
        var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Math.Min(Environment.ProcessorCount, 4)) };

        await Parallel.ForEachAsync(
            inputStreams.Select((stream, index) => (stream, index, fileName: fileNames[index])),
            options,
            async (item, cancellationToken) =>
            {
                try
                {
                    // Copy stream to MemoryStream for thread-safe processing
                    using var memoryStream = new MemoryStream();
                    await item.stream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Position = 0;

                    var result = await ProcessSingleAsync(model, memoryStream);

                    // Generate Filename Logic
                    string pattern = string.IsNullOrWhiteSpace(model.FilenamePattern) ? "{original}_processed" : model.FilenamePattern;

                    string baseName = Path.GetFileNameWithoutExtension(item.fileName);

                    // Basic replacements
                    string newName = pattern
                        .Replace("{original}", baseName)
                        .Replace("{name}", baseName) // Alias
                        .Replace("{fullname}", item.fileName)
                        .Replace("{date}", DateTime.Now.ToString("yyyyMMdd"))
                        .Replace("{time}", DateTime.Now.ToString("HHmmss"))
                        .Replace("{width}", result.ProcessedWidth.ToString())
                        .Replace("{height}", result.ProcessedHeight.ToString());

                    // Ensure correct extension matches the actual format
                    string finalName = Path.GetFileNameWithoutExtension(newName);
                    if (string.IsNullOrWhiteSpace(finalName)) finalName = "image_" + item.index;
                    finalName += "." + result.Format; // Force correct extension

                    // Sanitize filename
                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        finalName = finalName.Replace(c, '_');
                    }

                    var fullPath = Path.Combine(outputDirectory, finalName);

                    // Handle duplicates - simplistic approach
                    int counter = 1;
                    while (File.Exists(fullPath))
                    {
                        var name = Path.GetFileNameWithoutExtension(finalName);
                        var ext = Path.GetExtension(finalName);
                        fullPath = Path.Combine(outputDirectory, $"{name}_{counter++}{ext}");
                    }

                    await File.WriteAllBytesAsync(fullPath, result.Data, cancellationToken);

                    // Explicitly clear data reference to help GC? 
                    // result.Data = null; // not settable? It is settable.
                    // But result goes out of scope anyway.

                    savedFilePaths.Add(fullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing image {item.fileName} in batch");
                }
            });

        return savedFilePaths.ToList();
    }
}
