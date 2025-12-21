using System.ComponentModel.DataAnnotations;

namespace NovaToolsHub.Models.ViewModels;

public class EncryptionRequestViewModel
{
    [Required]
    [StringLength(50000, ErrorMessage = "Text too long (50,000 char max).")]
    public string Text { get; set; } = string.Empty;

    [Required]
    [StringLength(256, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 256 characters.")]
    public string Password { get; set; } = string.Empty;
}
