using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NovaToolsHub.Models.ViewModels
{
    // Unit Converter
    public class UnitConversionRequest
    {
        public double Value { get; set; }
        public string FromUnit { get; set; } = string.Empty;
        public string ToUnit { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    // Currency Converter
    public class CurrencyConversionRequest
    {
        public decimal Amount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
    }

    // BMI Calculator
    public class BmiRequest
    {
        public double Height { get; set; }
        public string HeightUnit { get; set; } = "cm"; // cm or m
        public double Weight { get; set; }
        public string WeightUnit { get; set; } = "kg"; // kg or lbs
    }

    // Age Calculator
    public class AgeRequest
    {
        public string BirthDate { get; set; } = string.Empty;
        public string? TargetDate { get; set; }
    }

    // Date Calculator
    public class DateDifferenceRequest
    {
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
    }

    public class DateAddSubtractRequest
    {
        public string StartDate { get; set; } = string.Empty;
        public int Days { get; set; }
        public string Operation { get; set; } = "add"; // add or subtract
    }

    // Password Generator
    public class PasswordGeneratorRequest
    {
        public int Length { get; set; } = 16;
        public bool IncludeUppercase { get; set; } = true;
        public bool IncludeLowercase { get; set; } = true;
        public bool IncludeNumbers { get; set; } = true;
        public bool IncludeSymbols { get; set; } = true;
    }

    // Advanced Image Resizer
    public class AdvancedImageResizerViewModel
    {
        public IFormFile? UploadedFile { get; set; }

        public List<IFormFile>? UploadedFiles { get; set; }

        public bool IsBatchMode { get; set; }

        // Per-tab apply toggles
        public bool ApplyResize { get; set; }

        public bool ApplyCrop { get; set; }

        public bool ApplyCompression { get; set; }

        public bool ApplyEffects { get; set; }

        public bool ApplyWatermark { get; set; }

        public int? TargetWidth { get; set; }

        public int? TargetHeight { get; set; }

        public bool MaintainAspectRatio { get; set; } = true;

        public int? ResizePercentage { get; set; }

        public int? Quality { get; set; } = 90;

        public long? TargetFileSizeKb { get; set; }

        public string OutputFormat { get; set; } = "original";

        public bool ApplyGrayscale { get; set; }

        public bool ApplySepia { get; set; }

        public bool PreserveMetadata { get; set; }

        public float? Brightness { get; set; } = 1.0f;

        public float? Contrast { get; set; } = 1.0f;

        public int? RotationDegrees { get; set; } = 0;

        public bool FlipHorizontal { get; set; }

        public bool FlipVertical { get; set; }

        public string CropPreset { get; set; } = "none";

        public string? FilenamePattern { get; set; } = "{original}_processed";

        public int? CropX { get; set; }

        public int? CropY { get; set; }

        public int? CropWidth { get; set; }

        public int? CropHeight { get; set; }

        public string? WatermarkText { get; set; }

        public string WatermarkPosition { get; set; } = "BottomRight";

        public float WatermarkOpacity { get; set; } = 0.5f;

        public string WatermarkFontFamily { get; set; } = "Arial";

        public int WatermarkFontSize { get; set; } = 32;

        public string WatermarkColor { get; set; } = "#FFFFFF";

        public IFormFile? WatermarkImageFile { get; set; }
    }

    // Processed Image Result
    public class ProcessedImageResult
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public string MimeType { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public int OriginalWidth { get; set; }

        public int OriginalHeight { get; set; }

        public long OriginalSizeBytes { get; set; }

        public int ProcessedWidth { get; set; }

        public int ProcessedHeight { get; set; }

        public long ProcessedSizeBytes { get; set; }

        public string Format { get; set; } = string.Empty;

        public string GetOriginalSizeFormatted()
        {
            return FormatFileSize(OriginalSizeBytes);
        }

        public string GetProcessedSizeFormatted()
        {
            return FormatFileSize(ProcessedSizeBytes);
        }

        public double GetCompressionRatio()
        {
            if (OriginalSizeBytes == 0) return 0;
            return Math.Round((1 - (double)ProcessedSizeBytes / OriginalSizeBytes) * 100, 2);
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    // Polls
    public class CreatePollRequest
    {
        [Required]
        [StringLength(200, ErrorMessage = "Question too long (200 chars max).")]
        public string Question { get; set; } = string.Empty;

        [Required]
        [MinLength(2, ErrorMessage = "At least 2 options required.")]
        [MaxLength(10, ErrorMessage = "Maximum 10 options allowed.")]
        public List<string> Options { get; set; } = new();
    }

    public class VoteRequest
    {
        [Required]
        public Guid PollId { get; set; }

        [Required]
        public Guid OptionId { get; set; }
    }

    public class PollResultDto
    {
        public Guid PollId { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<PollOptionResultDto> Options { get; set; } = new();
        public int TotalVotes => Options.Sum(o => o.Votes);
    }

    public class PollOptionResultDto
    {
        public Guid OptionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Votes { get; set; }
    }

    // AI Writing Assistant
    public class AiDraftRequest
    {
        [Required]
        [StringLength(4000, MinimumLength = 5, ErrorMessage = "Prompt must be between 5 and 4000 characters.")]
        public string Prompt { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(professional|casual|friendly|formal|creative|neutral)$", ErrorMessage = "Invalid tone selected.")]
        public string Tone { get; set; } = "neutral";

        [Required]
        [RegularExpression("^(short|medium|long)$", ErrorMessage = "Invalid length option.")]
        public string Length { get; set; } = "medium";
    }

    public class AiSummarizeRequest
    {
        [Required]
        [StringLength(6000, MinimumLength = 20, ErrorMessage = "Text must be between 20 and 6000 characters.")]
        public string Text { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(brief|medium|detailed)$", ErrorMessage = "Invalid summary length.")]
        public string Length { get; set; } = "medium";
    }

    public class AiImproveRequest
    {
        [Required]
        [StringLength(6000, MinimumLength = 20, ErrorMessage = "Text must be between 20 and 6000 characters.")]
        public string Text { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Goal is too long.")]
        public string? Goal { get; set; }
    }

    public class AiTextResponse
    {
        public string Mode { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Tone { get; set; } = string.Empty;
        public string Length { get; set; } = string.Empty;
        public int EstimatedTokens { get; set; }
        public List<string> Tips { get; set; } = new();
    }

    // AI Grammar Check
    public class GrammarCheckRequest
    {
        [Required]
        [StringLength(8000, MinimumLength = 10, ErrorMessage = "Text must be between 10 and 8000 characters.")]
        public string Text { get; set; } = string.Empty;
    }

    public class GrammarCheckResponse
    {
        public bool Success { get; set; }
        public string CorrectedText { get; set; } = string.Empty;
        public List<GrammarIssue> Issues { get; set; } = new();
        public int IssueCount => Issues.Count;
        public string Summary { get; set; } = string.Empty;
    }

    public class GrammarIssue
    {
        public string Type { get; set; } = string.Empty;  // grammar, spelling, punctuation, style
        public string Severity { get; set; } = "medium";  // low, medium, high
        public string Original { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }
}
