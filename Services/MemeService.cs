using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NovaToolsHub.Models.ViewModels;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NovaToolsHub.Services;

public class MemeService : IMemeService
{
    private const int MaxDimension = 2000;
    private const int CanvasPadding = 24;

    private static readonly Dictionary<string, string> FontLookup = new(StringComparer.OrdinalIgnoreCase)
    {
        ["impact"] = "Impact",
        ["arial"] = "Arial",
        ["montserrat"] = "Montserrat",
        ["opensans"] = "Open Sans",
        ["roboto"] = "Roboto"
    };

    private readonly ILogger<MemeService> _logger;

    public MemeService(ILogger<MemeService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MemeResponse> GenerateAsync(MemeRequest request, CancellationToken cancellationToken = default)
    {
        if (request.BaseImage == null || request.BaseImage.Length == 0)
        {
            throw new InvalidDataException("Upload an image to create your meme.");
        }

        if (string.IsNullOrWhiteSpace(request.TopText) && string.IsNullOrWhiteSpace(request.BottomText))
        {
            throw new InvalidDataException("Add top or bottom text to generate a meme.");
        }

        try
        {
            await using var inputStream = request.BaseImage.OpenReadStream();
            using var image = await Image.LoadAsync<Rgba32>(inputStream, cancellationToken);

            NormalizeCanvas(image);

            var fontFamily = ResolveFontFamily(request.FontFamily);
            var font = fontFamily.CreateFont(request.FontSize, FontStyle.Bold);
            var fillColor = ParseColor(request.FontColor);
            var strokeColor = GetStrokeColor(fillColor);
            var strokeWidth = Math.Clamp(request.StrokeWidth, 0, 6);

            DrawRegion(image, request.TopText, font, fillColor, strokeColor, strokeWidth, TextRegion.Top);
            DrawRegion(image, request.BottomText, font, fillColor, strokeColor, strokeWidth, TextRegion.Bottom);

            using var output = new MemoryStream();
            await image.SaveAsPngAsync(output, cancellationToken);

            return new MemeResponse
            {
                Base64Image = Convert.ToBase64String(output.ToArray()),
                ContentType = "image/png",
                FileName = $"nova-meme-{DateTime.UtcNow:yyyyMMdd_HHmmss}.png"
            };
        }
        catch (UnknownImageFormatException ex)
        {
            _logger.LogWarning(ex, "Unsupported image format uploaded for meme generation.");
            throw new InvalidDataException("Unsupported or corrupt image file. Please try another image.");
        }
        catch (ImageFormatException ex)
        {
            _logger.LogWarning(ex, "Invalid image uploaded for meme generation.");
            throw new InvalidDataException("Unsupported or corrupt image file. Please try another image.");
        }
    }

    private static void NormalizeCanvas(Image image)
    {
        var maxSide = Math.Max(image.Width, image.Height);
        if (maxSide <= MaxDimension)
        {
            return;
        }

        var ratio = MaxDimension / (double)maxSide;
        var targetWidth = Math.Max(1, (int)Math.Round(image.Width * ratio));
        var targetHeight = Math.Max(1, (int)Math.Round(image.Height * ratio));

        image.Mutate(ctx => ctx.Resize(targetWidth, targetHeight));
    }

    private static FontFamily ResolveFontFamily(string fontKey)
    {
        var lookupKey = fontKey?.Trim().ToLowerInvariant() ?? "impact";
        var preferred = FontLookup.TryGetValue(lookupKey, out var mapped) ? mapped : FontLookup["impact"];
        if (SystemFonts.TryGet(preferred, out var fontFamily))
        {
            return fontFamily;
        }

        return SystemFonts.CreateFont("Arial", 32).Family;
    }

    private static Color ParseColor(string hex)
    {
        if (!string.IsNullOrWhiteSpace(hex))
        {
            var value = hex.Trim();
            if (!value.StartsWith('#'))
            {
                value = "#" + value;
            }

            if (Regex.IsMatch(value, "^#[0-9A-Fa-f]{6}$"))
            {
                return Color.ParseHex(value);
            }
        }

        return Color.ParseHex("#FFFFFF");
    }

    private static Color GetStrokeColor(Color fill)
    {
        var rgba = fill.ToPixel<Rgba32>();
        var luminance = (0.299 * rgba.R + 0.587 * rgba.G + 0.114 * rgba.B) / 255d;
        return luminance > 0.6 ? Color.Black : Color.White;
    }

    private static void DrawRegion(Image image, string text, Font font, Color fill, Color stroke, float strokeWidth, TextRegion region)
    {
        var lines = WrapText(text, font, image.Width);
        if (lines.Count == 0)
        {
            return;
        }

        var lineHeight = font.Size * 1.2f;
        var margin = Math.Max(CanvasPadding, font.Size * 0.6f);
        float startY = region == TextRegion.Top
            ? margin
            : Math.Max(margin, image.Height - margin - (lineHeight * lines.Count));

        foreach (var line in lines)
        {
            var options = CreateTextOptions(font, image.Width, new PointF(image.Width / 2f, startY));
            DrawOutlinedText(image, options, line, fill, stroke, strokeWidth);
            startY += lineHeight;
        }
    }

    private static RichTextOptions CreateTextOptions(Font font, int canvasWidth, PointF origin)
    {
        return new RichTextOptions(font)
        {
            Origin = origin,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            WrappingLength = Math.Max(50, canvasWidth - (CanvasPadding * 2)),
            KerningMode = KerningMode.Standard
        };
    }

    private static void DrawOutlinedText(Image image, RichTextOptions options, string text, Color fill, Color stroke, float strokeWidth)
    {
        image.Mutate(ctx =>
        {
            if (strokeWidth > 0)
            {
                foreach (var offset in GetOutlineOffsets(strokeWidth))
                {
                    var outlineOptions = options;
                    outlineOptions.Origin = new PointF(options.Origin.X + offset.X, options.Origin.Y + offset.Y);
                    ctx.DrawText(outlineOptions, text, stroke);
                }
            }

            ctx.DrawText(options, text, fill);
        });
    }

    private static IReadOnlyList<PointF> GetOutlineOffsets(float strokeWidth)
    {
        var radius = Math.Max(1f, strokeWidth);
        return new List<PointF>
        {
            new(-radius, 0),
            new(radius, 0),
            new(0, -radius),
            new(0, radius),
            new(-radius, -radius),
            new(radius, radius),
            new(radius, -radius),
            new(-radius, radius)
        };
    }

    private static IReadOnlyList<string> WrapText(string? text, Font font, int canvasWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        var normalized = Regex.Replace(text.Trim(), @"\s+", " ").ToUpperInvariant();
        var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return Array.Empty<string>();
        }

        var effectiveWidth = Math.Max(100, canvasWidth - (CanvasPadding * 2));
        var lines = new List<string>();
        var currentLine = new StringBuilder();
        var measureOptions = new TextOptions(font)
        {
            WrappingLength = effectiveWidth,
            Dpi = 72,
            HorizontalAlignment = HorizontalAlignment.Left,
            KerningMode = KerningMode.Standard
        };

        foreach (var word in words)
        {
            var testLine = currentLine.Length == 0 ? word : $"{currentLine} {word}";
            var size = TextMeasurer.MeasureSize(testLine, measureOptions);
            if (size.Width > effectiveWidth && currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString());
                currentLine.Clear();
                currentLine.Append(word);
            }
            else
            {
                if (currentLine.Length > 0)
                {
                    currentLine.Append(' ');
                }

                currentLine.Append(word);
            }
        }

        if (currentLine.Length > 0)
        {
            lines.Add(currentLine.ToString());
        }

        return lines;
    }

    private enum TextRegion
    {
        Top,
        Bottom
    }
}
