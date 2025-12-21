namespace NovaToolsHub.Services;

/// <summary>
/// Represents a single AI provider option that can be used as a fallback.
/// </summary>
public class AiProviderConfig
{
    public string? Provider { get; set; }
    public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; }
    public string? Model { get; set; }
    public int? MaxTokens { get; set; }
    public double? Temperature { get; set; }
    public int? TimeoutSeconds { get; set; }
    public string? Referer { get; set; }
    public string? SiteName { get; set; }
    public bool? SendProviderHeaders { get; set; }
}
