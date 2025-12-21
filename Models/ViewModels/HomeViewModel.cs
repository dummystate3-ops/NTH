namespace NovaToolsHub.Models.ViewModels;

/// <summary>
/// Base view model for all pages with SEO metadata
/// </summary>
public class BasePageViewModel
{
    public string PageTitle { get; set; } = "NovaTools Hub";
    public string MetaDescription { get; set; } = "NovaTools Hub - Your comprehensive suite of calculators and tools.";
    public string CanonicalUrl { get; set; } = string.Empty;
    public string OgImage { get; set; } = "/images/og-default.png";
    public string OgType { get; set; } = "website";
    public Dictionary<string, object>? JsonLdSchema { get; set; }
}

/// <summary>
/// Home page view model
/// </summary>
public class HomeViewModel : BasePageViewModel
{
    public List<ToolCategory> ToolCategories { get; set; } = new();
    public List<FeaturedTool> FeaturedTools { get; set; } = new();
}

/// <summary>
/// Tool category for grouping
/// </summary>
public class ToolCategory
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty; // CSS class or emoji
    public int ToolCount { get; set; }
}

/// <summary>
/// Featured tool display
/// </summary>
public class FeaturedTool
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
