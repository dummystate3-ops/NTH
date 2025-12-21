namespace NovaToolsHub.Services;

/// <summary>
/// Configuration settings for the Background Removal service.
/// </summary>
public class BackgroundRemovalSettings
{
    /// <summary>
    /// Path to the general U2Net ONNX model file (relative to ContentRootPath).
    /// Default: "App_Data/models/u2net.onnx"
    /// </summary>
    public string ModelPathGeneral { get; set; } = "App_Data/models/u2net.onnx";

    /// <summary>
    /// Path to the portrait (human segmentation) ONNX model file (relative to ContentRootPath).
    /// Default: "App_Data/models/u2net_human_seg.onnx"
    /// </summary>
    public string ModelPathPortrait { get; set; } = "App_Data/models/u2net_human_seg.onnx";

    /// <summary>
    /// Legacy single model path (used as fallback for general).
    /// </summary>
    public string? ModelPath { get; set; }

    /// <summary>
    /// Maximum number of concurrent inference operations.
    /// Higher values use more memory but process faster under load.
    /// Default: 2
    /// </summary>
    public int MaxConcurrentInferences { get; set; } = 2;

    /// <summary>
    /// Whether to log telemetry (inference time, queue depth, output size).
    /// Default: true
    /// </summary>
    public bool EnableTelemetry { get; set; } = true;
}
