using System.Collections.Generic;

namespace NovaToolsHub.Services;

public class AiSettings
{
    /// <summary>
    /// Provider key, e.g., "OpenAI" or "Mock".
    /// </summary>
    public string Provider { get; set; } = "Mock";

    /// <summary>
    /// API key for the provider.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Base URL for the provider (default: https://api.openai.com/v1/).
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Model name (default: gpt-3.5-turbo).
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Optional maximum tokens.
    /// </summary>
    public int? MaxTokens { get; set; } = 600;

    /// <summary>
    /// Temperature for sampling (default: 0.7).
    /// </summary>
    public double? Temperature { get; set; } = 0.7;

    /// <summary>
    /// Timeout in seconds for API calls.
    /// </summary>
    public int? TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Optional Referer header (OpenRouter recommends your site URL).
    /// </summary>
    public string? Referer { get; set; }

    /// <summary>
    /// Optional X-Title header (OpenRouter recommends your app name).
    /// </summary>
    public string? SiteName { get; set; }

    /// <summary>
    /// Toggle sending provider headers (Referer/X-Title) for OpenRouter-style providers.
    /// </summary>
    public bool SendProviderHeaders { get; set; } = true;

    /// <summary>
    /// Additional providers to try if the primary provider fails.
    /// </summary>
    public List<AiProviderConfig> FallbackProviders { get; set; } = new();
}
