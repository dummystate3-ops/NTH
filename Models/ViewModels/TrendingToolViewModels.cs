using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using NovaToolsHub.Helpers;

namespace NovaToolsHub.Models.ViewModels;

public class MemeRequest
{
    [Required]
    [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }, ErrorMessage = "Only JPG, PNG, GIF, and WEBP images are allowed.")]
    [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Image must be 5 MB or smaller.")]
    public IFormFile? BaseImage { get; set; }

    [StringLength(120, ErrorMessage = "Top text is too long (max 120 characters).")]
    public string TopText { get; set; } = string.Empty;

    [StringLength(120, ErrorMessage = "Bottom text is too long (max 120 characters).")]
    public string BottomText { get; set; } = string.Empty;

    [Range(12, 96, ErrorMessage = "Font size must be between 12 and 96.")]
    public int FontSize { get; set; } = 32;

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Font color must be a hex value like #FFFFFF.")]
    public string FontColor { get; set; } = "#FFFFFF";

    [RegularExpression("^(impact|arial|montserrat|opensans|roboto)$", ErrorMessage = "Invalid font choice.")]
    public string FontFamily { get; set; } = "impact";

    [Range(0, 4, ErrorMessage = "Stroke width must be between 0 and 4.")]
    public int StrokeWidth { get; set; } = 2;
}

public class MemeResponse
{
    public string Base64Image { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/png";
    public string FileName { get; set; } = "meme.png";
    public string? DownloadKey { get; set; }
}

public class RecipeRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Add at least one ingredient.")]
    [MaxLength(20, ErrorMessage = "Too many ingredients (max 20).")]
    public List<string> Ingredients { get; set; } = new();

    [RegularExpression("^(none|vegan|vegetarian|gluten-free|keto|paleo|low-carb)$", ErrorMessage = "Unsupported dietary preference.")]
    public string DietaryPreference { get; set; } = "none";

    [Range(1, 16, ErrorMessage = "Servings must be between 1 and 16.")]
    public int Servings { get; set; } = 2;

    [StringLength(120, ErrorMessage = "Cuisine name too long (max 120 characters).")]
    public string? Cuisine { get; set; }

    [StringLength(200, ErrorMessage = "Allergy notes too long (max 200 characters).")]
    public string? Allergies { get; set; }

    [RegularExpression("^(quick|medium|long|any)$", ErrorMessage = "Invalid cooking time preference.")]
    public string CookingTimePreference { get; set; } = "any";
}

public class RecipeResponse
{
    public string Title { get; set; } = string.Empty;
    public List<string> Ingredients { get; set; } = new();
    public List<string> Steps { get; set; } = new();
    public int Servings { get; set; }
    public string? DietaryPreference { get; set; }
    public string? Cuisine { get; set; }
    public int PrepMinutes { get; set; }
    public int CookMinutes { get; set; }
    public int TotalMinutes { get; set; }
    public string Difficulty { get; set; } = "Easy";
    public RecipeNutrition Nutrition { get; set; } = new();
    public string? Notes { get; set; }

    // Enhanced metadata for analytics and UX
    public string RecipeId { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public bool IsFallbackRecipe { get; set; }
    public string ImagePrompt { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string? EquipmentNotes { get; set; }
}

public class RecipeNutrition
{
    public int Calories { get; set; }
    public int ProteinGrams { get; set; }
    public int CarbsGrams { get; set; }
    public int FatGrams { get; set; }
    public int FiberGrams { get; set; }
    public int SugarGrams { get; set; }
    public int SodiumMilligrams { get; set; }
}

public class RateComparisonRequest
{
    [Required]
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Base currency must be a 3-letter code.")]
    public string BaseCurrency { get; set; } = "USD";

    [Required]
    [MinLength(1, ErrorMessage = "Add at least one comparison row.")]
    [MaxLength(10, ErrorMessage = "Too many rows (max 10).")]
    public List<RateComparisonItem> Targets { get; set; } = new();
}

public class RateComparisonItem
{
    [Required]
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency must be a 3-letter code.")]
    public string Currency { get; set; } = string.Empty;

    [Range(0.0001, 1_000_000, ErrorMessage = "Amount must be between 0.0001 and 1,000,000.")]
    public decimal Amount { get; set; } = 1m;
}

public class RateComparisonResponse
{
    public string BaseCurrency { get; set; } = string.Empty;
    public DateTime RetrievedAtUtc { get; set; } = DateTime.UtcNow;
    public List<RateComparisonRow> Rows { get; set; } = new();
    public bool UsedFallbackRates { get; set; }
    public bool UsedHistoricalFallback { get; set; }
}

public class RateComparisonRow
{
    public string Currency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal? Change24h { get; set; }
    public decimal? Change7d { get; set; }
}
