using NovaToolsHub.Models.ViewModels;

namespace NovaToolsHub.Services;

/// <summary>
/// Interface for image processing operations including resizing, compression, and transformations
/// </summary>
public interface IImageProcessingService
{
    /// <summary>
    /// Process a single image based on the provided parameters
    /// </summary>
    /// <param name="model">The view model containing processing parameters</param>
    /// <param name="inputStream">The input image stream</param>
    /// <returns>Processed image result with data and metadata</returns>
    Task<ProcessedImageResult> ProcessSingleAsync(AdvancedImageResizerViewModel model, Stream inputStream);

    /// <summary>
    /// Generate a fast low-resolution preview of the image with effects applied
    /// </summary>
    /// <param name="model">The view model containing processing parameters</param>
    /// <param name="inputStream">The input image stream</param>
    /// <param name="maxDimension">Maximum width/height of preview (default 400px)</param>
    /// <returns>Processed image result with preview data</returns>
    Task<ProcessedImageResult> GeneratePreviewAsync(AdvancedImageResizerViewModel model, Stream inputStream, int maxDimension = 400);

    /// <summary>
    /// Process multiple images in batch mode (parallel processing)
    /// </summary>
    /// <param name="model">The view model containing processing parameters</param>
    /// <param name="inputStreams">List of input image streams</param>
    /// <param name="fileNames">List of file names corresponding to each stream</param>
    /// <returns>List of processed image results</returns>
    Task<List<ProcessedImageResult>> ProcessBatchAsync(AdvancedImageResizerViewModel model, List<Stream> inputStreams, List<string> fileNames);

    /// <summary>
    /// Process batch and save directly to disk (memory efficient)
    /// </summary>
    Task<List<string>> ProcessBatchAndSaveAsync(AdvancedImageResizerViewModel model, List<Stream> inputStreams, List<string> fileNames, string outputDirectory);
}

