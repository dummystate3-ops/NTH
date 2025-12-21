using System.Text.Json;

namespace NovaToolsHub.Services;

/// <summary>
/// Generic API service for external integrations
/// Provides abstraction for various API calls (AI, plagiarism, etc.)
/// </summary>
public interface IApiService
{
    Task<string> CallAiApiAsync(string prompt, string endpoint = "completions");
    Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null);
    Task<T?> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null);
}

public class ApiService : IApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiService> _logger;

    public ApiService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<string> CallAiApiAsync(string prompt, string endpoint = "completions")
    {
        try
        {
            var apiKey = _configuration["ApiSettings:OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("OpenAI API key not configured. Returning mock response.");
                return Task.FromResult(GetMockAiResponse(prompt));
            }

            // TODO: Implement actual OpenAI API call
            // var httpClient = _httpClientFactory.CreateClient();
            // Add authorization header with API key
            // Make POST request to API endpoint

            return Task.FromResult(GetMockAiResponse(prompt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AI API");
            return Task.FromResult(GetMockAiResponse(prompt));
        }
    }

    public async Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GET request to {Url}", url);
            return default;
        }
    }

    public async Task<T?> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in POST request to {Url}", url);
            return default;
        }
    }

    private string GetMockAiResponse(string prompt)
    {
        // Mock AI response for development
        return $"This is a mock AI response. Configure your API key in appsettings.json to use real AI services. Your prompt was: {prompt.Substring(0, Math.Min(50, prompt.Length))}...";
    }
}
