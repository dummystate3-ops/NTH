using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NovaToolsHub.Services;

/// <summary>
/// Background removal service using ONNX Runtime with U2Net model.
/// Provides AI-powered background removal running entirely locally (zero API costs).
/// </summary>
public class BackgroundRemovalService : IBackgroundRemovalService, IDisposable
{
    private readonly ILogger<BackgroundRemovalService> _logger;
    private readonly BackgroundRemovalSettings _settings;
    private readonly string _generalModelPath;
    private readonly string _portraitModelPath;
    private readonly Dictionary<BackgroundRemovalMode, InferenceSession> _sessions = new();
    private readonly object _lock = new();
    private bool _disposed;

    // Concurrency limiting (Phase 2)
    private readonly SemaphoreSlim _inferenceSemaphore;

    private const int MODEL_INPUT_SIZE = 320; // U2Net standard input size
    private const long MAX_PIXEL_COUNT = 40_000_000; // 40 megapixels max to prevent decompression bombs
    private const int PORTRAIT_ALPHA_BASE_LOW = 8; // Start fading in alpha (reduce background haze)
    private const int PORTRAIT_ALPHA_BASE_HIGH = 40; // Full alpha retention threshold
    private const int PORTRAIT_ALPHA_LOW_RANGE = 6; // +/- range for edge strength slider
    private const int PORTRAIT_ALPHA_HIGH_RANGE = 20; // +/- range for edge strength slider
    private const float PORTRAIT_ALPHA_GAMMA_BASE = 1.0f; // Neutral gamma
    private const float PORTRAIT_ALPHA_GAMMA_RANGE = 0.1f; // +/- gamma range for edge strength slider

    public BackgroundRemovalService(
        ILogger<BackgroundRemovalService> logger,
        IWebHostEnvironment environment,
        IOptions<BackgroundRemovalSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;

        // Configurable model paths (moved out of wwwroot)
        var generalPath = string.IsNullOrWhiteSpace(_settings.ModelPathGeneral)
            ? _settings.ModelPath ?? "App_Data/models/u2net.onnx"
            : _settings.ModelPathGeneral;

        _generalModelPath = Path.IsPathRooted(generalPath)
            ? generalPath
            : Path.Combine(environment.ContentRootPath, generalPath);

        var portraitPath = string.IsNullOrWhiteSpace(_settings.ModelPathPortrait)
            ? "App_Data/models/u2net_human_seg.onnx"
            : _settings.ModelPathPortrait;

        _portraitModelPath = Path.IsPathRooted(portraitPath)
            ? portraitPath
            : Path.Combine(environment.ContentRootPath, portraitPath);

        _inferenceSemaphore = new SemaphoreSlim(
            _settings.MaxConcurrentInferences,
            _settings.MaxConcurrentInferences);

        _logger.LogInformation(
            "BackgroundRemovalService initialized. GeneralModelPath: {GeneralPath}, PortraitModelPath: {PortraitPath}, MaxConcurrency: {Max}",
            _generalModelPath, _portraitModelPath, _settings.MaxConcurrentInferences);
    }

    public string GeneralModelPath => _generalModelPath;

    public string PortraitModelPath => _portraitModelPath;

    public bool IsGeneralModelLoaded => File.Exists(_generalModelPath);

    public bool IsPortraitModelLoaded => File.Exists(_portraitModelPath);

    /// <summary>
    /// Removes the background from an image using U2Net segmentation model.
    /// Uses letterbox preprocessing to preserve aspect ratio.
    /// </summary>
    public async Task<byte[]> RemoveBackgroundAsync(
        byte[] imageData,
        BackgroundRemovalMode mode = BackgroundRemovalMode.General,
        int? portraitEdgeStrength = null,
        CancellationToken cancellationToken = default)
    {
        EnsureModelLoaded(mode);

        var stopwatch = _settings.EnableTelemetry ? Stopwatch.StartNew() : null;
        var queueDepth = _settings.MaxConcurrentInferences - _inferenceSemaphore.CurrentCount;

        if (_settings.EnableTelemetry)
        {
            _logger.LogDebug("Background removal queued. QueueDepth: {Depth}", queueDepth);
        }

        // Acquire semaphore (concurrency limiting)
        await _inferenceSemaphore.WaitAsync(cancellationToken);

        try
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var inputImage = Image.Load<Rgba32>(imageData);
                var originalWidth = inputImage.Width;
                var originalHeight = inputImage.Height;

                // Validate pixel count to prevent decompression bombs
                var pixelCount = (long)originalWidth * originalHeight;
                if (pixelCount > MAX_PIXEL_COUNT)
                {
                    throw new InvalidOperationException(
                        $"Image too large: {pixelCount:N0} pixels exceeds maximum of {MAX_PIXEL_COUNT:N0} pixels.");
                }

                // Phase 2: Letterbox preprocessing to preserve aspect ratio
                var (paddedImage, padding) = CreateLetterboxImage(inputImage, MODEL_INPUT_SIZE);
                using var _ = paddedImage;

                // Convert image to tensor (normalized 0-1, RGB channels)
                var inputTensor = ImageToTensor(paddedImage);

                cancellationToken.ThrowIfCancellationRequested();

                // Run inference
                var session = _sessions[mode];
                var inputName = session.InputMetadata.Keys.First();
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
                };

                using var results = session.Run(inputs);

                // Select output by shape validation (U2Net outputs [1,1,H,W])
                var outputTensor = SelectMaskOutput(results, MODEL_INPUT_SIZE);

                // Create mask from output (still at 320x320)
                using var fullMask = CreateMaskFromOutput(outputTensor, MODEL_INPUT_SIZE, MODEL_INPUT_SIZE);

                // Phase 2: Unpad the mask to original aspect ratio region
                using var croppedMask = CropFromLetterbox(fullMask, padding, originalWidth, originalHeight);

                // Refine mask for portrait mode (reduce faint background shadows)
                RefineMask(croppedMask, mode, portraitEdgeStrength);

                // Apply mask to original image (create transparent background)
                ApplyMaskToImage(inputImage, croppedMask);

                // Encode as PNG (supports transparency)
                using var outputStream = new MemoryStream();
                inputImage.SaveAsPng(outputStream);
                var result = outputStream.ToArray();

                // Phase 3: Telemetry
                if (_settings.EnableTelemetry && stopwatch != null)
                {
                    stopwatch.Stop();
                    _logger.LogInformation(
                        "Background removal complete. Duration: {Duration}ms, InputSize: {InputW}x{InputH}, OutputSize: {OutputBytes} bytes",
                        stopwatch.ElapsedMilliseconds, originalWidth, originalHeight, result.Length);
                }

                return result;
            }, cancellationToken);
        }
        finally
        {
            _inferenceSemaphore.Release();
        }
    }

    /// <summary>
    /// Creates a letterboxed (padded) image that fits within the target size while preserving aspect ratio.
    /// </summary>
    private static (Image<Rgba32> paddedImage, LetterboxPadding padding) CreateLetterboxImage(
        Image<Rgba32> source, int targetSize)
    {
        int srcWidth = source.Width;
        int srcHeight = source.Height;

        float scale = Math.Min((float)targetSize / srcWidth, (float)targetSize / srcHeight);
        int scaledWidth = (int)(srcWidth * scale);
        int scaledHeight = (int)(srcHeight * scale);

        int padLeft = (targetSize - scaledWidth) / 2;
        int padTop = (targetSize - scaledHeight) / 2;

        var paddedImage = new Image<Rgba32>(targetSize, targetSize, new Rgba32(0, 0, 0, 255));
        using var resized = source.Clone(ctx => ctx.Resize(scaledWidth, scaledHeight));
        paddedImage.Mutate(ctx => ctx.DrawImage(resized, new Point(padLeft, padTop), 1f));

        var padding = new LetterboxPadding
        {
            Left = padLeft,
            Top = padTop,
            ScaledWidth = scaledWidth,
            ScaledHeight = scaledHeight,
            Scale = scale
        };

        return (paddedImage, padding);
    }

    /// <summary>
    /// Crops the mask from the letterboxed region and resizes to original dimensions.
    /// </summary>
    private static Image<L8> CropFromLetterbox(Image<L8> mask, LetterboxPadding padding, int originalWidth, int originalHeight)
    {
        var cropRect = new Rectangle(padding.Left, padding.Top, padding.ScaledWidth, padding.ScaledHeight);
        using var cropped = mask.Clone(ctx => ctx.Crop(cropRect));
        return cropped.Clone(ctx => ctx.Resize(originalWidth, originalHeight));
    }

    private struct LetterboxPadding
    {
        public int Left;
        public int Top;
        public int ScaledWidth;
        public int ScaledHeight;
        public float Scale;
    }

    /// <summary>
    /// Selects the mask output tensor from ONNX results by validating shape.
    /// U2Net outputs multiple tensors; we select the one matching [1,1,H,W].
    /// </summary>
    private Tensor<float> SelectMaskOutput(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results, int expectedSize)
    {
        foreach (var result in results)
        {
            var tensor = result.AsTensor<float>();
            if (tensor == null) continue;

            var dims = tensor.Dimensions.ToArray();
            // U2Net mask output is typically [1, 1, 320, 320]
            if (dims.Length == 4 && dims[0] == 1 && dims[1] == 1 &&
                dims[2] == expectedSize && dims[3] == expectedSize)
            {
                _logger.LogDebug("Selected ONNX output '{Name}' with shape [{Dims}]",
                    result.Name, string.Join(",", dims));
                return tensor;
            }
        }

        // Fallback: try first output with warning
        var first = results.First();
        var firstTensor = first.AsTensor<float>();
        var firstDims = firstTensor?.Dimensions.ToArray() ?? Array.Empty<int>();

        _logger.LogWarning(
            "No output matched expected shape [1,1,{Size},{Size}]. Using first output '{Name}' with shape [{Dims}]",
            expectedSize, expectedSize, first.Name, string.Join(",", firstDims));

        return firstTensor ?? throw new InvalidOperationException("No valid tensor output from ONNX model.");
    }

    private void EnsureModelLoaded(BackgroundRemovalMode mode)
    {
        if (_sessions.ContainsKey(mode)) return;

        lock (_lock)
        {
            if (_sessions.ContainsKey(mode)) return;

            var modelPath = mode == BackgroundRemovalMode.Portrait ? _portraitModelPath : _generalModelPath;
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException(
                    "U2Net model not found at configured path. Please ensure the model exists.");
            }

            _logger.LogInformation("Loading U2Net model for {Mode} from {ModelPath}", mode, modelPath);
            var sessionOptions = new Microsoft.ML.OnnxRuntime.SessionOptions();
            sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
            var session = new InferenceSession(modelPath, sessionOptions);
            _sessions[mode] = session;
            _logger.LogInformation("U2Net model loaded successfully for {Mode}", mode);
        }
    }

    private static DenseTensor<float> ImageToTensor(Image<Rgba32> image)
    {
        var tensor = new DenseTensor<float>(new[] { 1, 3, image.Height, image.Width });
        float[] mean = { 0.485f, 0.456f, 0.406f };
        float[] std = { 0.229f, 0.224f, 0.225f };

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    var pixel = row[x];
                    tensor[0, 0, y, x] = ((pixel.R / 255f) - mean[0]) / std[0];
                    tensor[0, 1, y, x] = ((pixel.G / 255f) - mean[1]) / std[1];
                    tensor[0, 2, y, x] = ((pixel.B / 255f) - mean[2]) / std[2];
                }
            }
        });

        return tensor;
    }

    private static Image<L8> CreateMaskFromOutput(Tensor<float> output, int width, int height)
    {
        var mask = new Image<L8>(width, height);

        // Auto-detect output range to avoid over-soft masks:
        // - If values are already normalized [0..1], clamp directly.
        // - Otherwise, fall back to min/max normalization (legacy behavior).
        float min = float.MaxValue;
        float max = float.MinValue;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var value = output[0, 0, y, x];
                if (value < min) min = value;
                if (value > max) max = value;
            }
        }

        var isNormalized = min >= 0f && max <= 1f;
        var range = max - min;
        if (!isNormalized && range < 0.0001f) range = 1f;

        mask.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    var value = output[0, 0, y, x];
                    var normalized = isNormalized
                        ? Math.Clamp(value, 0f, 1f)
                        : Math.Clamp((value - min) / range, 0f, 1f);
                    row[x] = new L8((byte)(normalized * 255));
                }
            }
        });

        return mask;
    }

    private static void RefineMask(Image<L8> mask, BackgroundRemovalMode mode, int? portraitEdgeStrength)
    {
        if (mode != BackgroundRemovalMode.Portrait)
        {
            return;
        }

        var strength = Math.Clamp(portraitEdgeStrength ?? 50, 0, 100);
        var delta = (strength - 50) / 50f;
        var low = Math.Clamp(PORTRAIT_ALPHA_BASE_LOW + (int)Math.Round(delta * PORTRAIT_ALPHA_LOW_RANGE), 0, 254);
        var high = Math.Clamp(PORTRAIT_ALPHA_BASE_HIGH + (int)Math.Round(delta * PORTRAIT_ALPHA_HIGH_RANGE), low + 1, 255);
        var gamma = PORTRAIT_ALPHA_GAMMA_BASE + (delta * PORTRAIT_ALPHA_GAMMA_RANGE);

        mask.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    var alpha = row[x].PackedValue;
                    if (alpha <= low)
                    {
                        row[x] = new L8(0);
                        continue;
                    }

                    float normalized;
                    if (alpha < high)
                    {
                        var t = (alpha - low) / (float)(high - low);
                        var smooth = t * t * (3f - 2f * t);
                        normalized = smooth;
                    }
                    else
                    {
                        normalized = alpha / 255f;
                    }

                    var adjusted = MathF.Pow(normalized, gamma);
                    row[x] = new L8((byte)Math.Clamp(adjusted * 255f, 0f, 255f));
                }
            }
        });
    }

    private static void ApplyMaskToImage(Image<Rgba32> image, Image<L8> mask)
    {
        image.ProcessPixelRows(mask, (imageAccessor, maskAccessor) =>
        {
            for (int y = 0; y < imageAccessor.Height; y++)
            {
                var imageRow = imageAccessor.GetRowSpan(y);
                var maskRow = maskAccessor.GetRowSpan(y);

                for (int x = 0; x < imageRow.Length; x++)
                {
                    var alpha = maskRow[x].PackedValue;
                    imageRow[x] = new Rgba32(imageRow[x].R, imageRow[x].G, imageRow[x].B, alpha);
                }
            }
        });
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var session in _sessions.Values)
        {
            session.Dispose();
        }
        _sessions.Clear();
        _inferenceSemaphore.Dispose();
    }
}
