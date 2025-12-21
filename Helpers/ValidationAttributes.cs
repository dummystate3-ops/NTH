using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NovaToolsHub.Helpers;

/// <summary>
/// Validates the maximum file size for uploaded files.
/// </summary>
public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly long _maxBytes;

    public MaxFileSizeAttribute(long maxBytes)
    {
        _maxBytes = maxBytes;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            return ValidateFile(file);
        }

        if (value is IEnumerable<IFormFile> files)
        {
            foreach (var uploadedFile in files)
            {
                var result = ValidateFile(uploadedFile);
                if (result != ValidationResult.Success)
                {
                    return result;
                }
            }
        }

        return ValidationResult.Success;
    }

    private ValidationResult? ValidateFile(IFormFile upload)
    {
        if (upload.Length > _maxBytes)
        {
            var mbLimit = Math.Round(_maxBytes / 1024d / 1024d, 2);
            var message = ErrorMessage ?? $"File size must be {mbLimit} MB or smaller.";
            return new ValidationResult(message);
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates allowed file extensions for uploads.
/// </summary>
public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly HashSet<string> _extensions;

    public AllowedExtensionsAttribute(string[] extensions)
    {
        _extensions = extensions
            .Select(ext => ext.StartsWith(".") ? ext.ToLowerInvariant() : $".{ext.ToLowerInvariant()}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            return ValidateFile(file);
        }

        if (value is IEnumerable<IFormFile> files)
        {
            foreach (var uploadedFile in files)
            {
                var result = ValidateFile(uploadedFile);
                if (result != ValidationResult.Success)
                {
                    return result;
                }
            }
        }

        return ValidationResult.Success;
    }

    private ValidationResult? ValidateFile(IFormFile upload)
    {
        var extension = Path.GetExtension(upload.FileName).ToLowerInvariant();
        if (!_extensions.Contains(extension))
        {
            var message = ErrorMessage ?? $"Invalid file type. Allowed: {string.Join(", ", _extensions)}";
            return new ValidationResult(message);
        }

        return ValidationResult.Success;
    }
}
