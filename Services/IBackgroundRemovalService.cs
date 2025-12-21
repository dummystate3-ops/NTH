namespace NovaToolsHub.Services;

/// <summary>
/// Service interface for AI-powered background removal from images.
/// Uses ONNX Runtime with U2Net model for local, zero-cost processing.
/// </summary>
public interface IBackgroundRemovalService
{
    /// <summary>
    /// Removes the background from an image, returning a PNG with transparent background.
    /// </summary>
    /// <param name="imageData">The input image as byte array.</param>
    /// <param name="mode">Background removal mode (general or portrait).</param>
    /// <param name="portraitEdgeStrength">Portrait edge strength (0-100). Only applies to portrait mode.</param>
    /// <param name="cancellationToken">Cancellation token for request cancellation.</param>
    /// <returns>Processed image as PNG byte array with transparent background.</returns>
    Task<byte[]> RemoveBackgroundAsync(
        byte[] imageData,
        BackgroundRemovalMode mode = BackgroundRemovalMode.General,
        int? portraitEdgeStrength = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the general ONNX model is available.
    /// </summary>
    bool IsGeneralModelLoaded { get; }

    /// <summary>
    /// Checks if the portrait ONNX model is available.
    /// </summary>
    bool IsPortraitModelLoaded { get; }

    /// <summary>
    /// Gets the general model file path.
    /// </summary>
    string GeneralModelPath { get; }

    /// <summary>
    /// Gets the portrait model file path.
    /// </summary>
    string PortraitModelPath { get; }
}
