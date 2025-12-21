using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NovaToolsHub.Models.ViewModels;

namespace NovaToolsHub.Services;

/// <summary>
/// OpenAI-backed implementation for AI writing flows. Falls back to the mock if not configured by DI factory.
/// </summary>
public class OpenAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly AiSettings _settings;
    private readonly ILogger<OpenAiService> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public OpenAiService(HttpClient httpClient, IOptions<AiSettings> options, ILogger<OpenAiService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;

        if (_httpClient.BaseAddress == null)
        {
            var baseUrl = string.IsNullOrWhiteSpace(_settings.BaseUrl) ? "https://api.openai.com/v1/" : _settings.BaseUrl;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        if (_settings.TimeoutSeconds is > 0)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds.Value);
        }
    }

    public Task<AiTextResponse> DraftAsync(AiDraftRequest request)
    {
        var prompt = $"Draft content about: {request.Prompt}. Tone: {request.Tone}. Length: {request.Length}. " +
                     "Return plain text paragraphs without markdown.";
        return SendChatAsync("draft", prompt, request.Tone, request.Length);
    }

    public Task<AiTextResponse> SummarizeAsync(AiSummarizeRequest request)
    {
        var instructions = request.Length switch
        {
            "brief" => "2 sentences max.",
            "detailed" => "5-7 sentences.",
            _ => "3-4 sentences."
        };

        var prompt = $"Summarize the following text in {request.Length} detail ({instructions}). " +
                     "Focus on key ideas and outcomes. Return plain text only.\n\n" +
                     request.Text;

        return SendChatAsync("summarize", prompt, "neutral", request.Length);
    }

    public Task<AiTextResponse> ImproveAsync(AiImproveRequest request)
    {
        var goal = string.IsNullOrWhiteSpace(request.Goal) ? string.Empty : $"Goal: {request.Goal}. ";
        var prompt = $"{goal}Rewrite the text to improve clarity, flow, and correctness. Keep the same meaning. " +
                     "Return plain text only.\n\n" +
                     request.Text;

        return SendChatAsync("improve", prompt, "neutral", "medium");
    }

    public Task<GrammarCheckResponse> CheckGrammarAsync(GrammarCheckRequest request)
    {
        return ExecuteWithProvidersAsync(
            provider => CheckGrammarWithProviderAsync(provider, request),
            "grammar-check");
    }

    private async Task<GrammarCheckResponse> CheckGrammarWithProviderAsync(ResolvedProvider provider, GrammarCheckRequest request)
    {
        var prompt = @"You are an expert grammar and writing assistant. Analyze the following text for grammar, spelling, punctuation, and style issues.

Return your response in this EXACT format:
CORRECTED:
[The fully corrected version of the text]

ISSUES:
[For each issue, on a new line:]
TYPE|SEVERITY|ORIGINAL|SUGGESTION|EXPLANATION

Where TYPE is one of: grammar, spelling, punctuation, style
Where SEVERITY is one of: low, medium, high

Example issue line:
spelling|high|recieve|receive|The correct spelling is 'receive' (i before e except after c)

If there are no issues, write ""No issues found"" after ISSUES:

TEXT TO CHECK:
" + request.Text;

        var desiredMaxTokens = _settings.MaxTokens ?? 1500;
        var payload = new
        {
            model = provider.Model,
            temperature = 0.3,
            max_tokens = provider.MaxTokens > desiredMaxTokens ? provider.MaxTokens : desiredMaxTokens,
            messages = new[]
            {
                new { role = "system", content = "You are a meticulous grammar checker. Be thorough but not overly pedantic. Focus on actual errors and significant style improvements." },
                new { role = "user", content = prompt }
            }
        };

        using var httpRequest = BuildRequest(provider, payload);
        var body = await SendAsync(provider, httpRequest, "grammar-check");
        return ParseGrammarResponse(body);
    }

    private async Task<AiTextResponse> SendChatWithProviderAsync(ResolvedProvider provider, string mode, string userPrompt, string tone, string length)
    {
        var payload = new
        {
            model = provider.Model,
            temperature = provider.Temperature,
            max_tokens = provider.MaxTokens,
            messages = new[]
            {
                new { role = "system", content = "You are a concise, helpful writing assistant. Respond with plain text only (no markdown)." },
                new { role = "user", content = userPrompt }
            }
        };

        using var request = BuildRequest(provider, payload);
        var body = await SendAsync(provider, request, $"chat::{mode}");
        return ParseResponse(mode, tone, length, body);
    }

    private HttpRequestMessage BuildRequest(ResolvedProvider provider, object payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(provider.BaseUri, "chat/completions"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, SerializerOptions), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", provider.ApiKey);

        if (provider.SendProviderHeaders && !string.IsNullOrWhiteSpace(provider.Referer))
        {
            request.Headers.TryAddWithoutValidation("Referer", provider.Referer);
        }
        if (provider.SendProviderHeaders && !string.IsNullOrWhiteSpace(provider.SiteName))
        {
            request.Headers.TryAddWithoutValidation("X-Title", provider.SiteName);
        }

        return request;
    }

    private async Task<string> SendAsync(ResolvedProvider provider, HttpRequestMessage request, string operation)
    {
        if (_httpClient.Timeout != provider.Timeout)
        {
            _httpClient.Timeout = provider.Timeout;
        }

        var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var reqId = response.Headers.TryGetValues("x-request-id", out var ids) ? ids.FirstOrDefault() : null;
            var trimmedBody = body.Length > 1200 ? body[..1200] + "…" : body;
            _logger.LogWarning("AI provider {Provider} failed ({Status}) during {Operation}. RequestId={RequestId} Body={Body}",
                provider.Name,
                response.StatusCode,
                operation,
                reqId ?? "n/a",
                trimmedBody);

            throw new InvalidOperationException($"Provider {provider.Name} returned an error ({response.StatusCode}).");
        }

        return body;
    }

    private async Task<T> ExecuteWithProvidersAsync<T>(Func<ResolvedProvider, Task<T>> action, string operationName)
    {
        var providers = ResolveProviders().ToList();
        if (providers.Count == 0)
        {
            throw new InvalidOperationException("No AI providers are configured. Set AI:ApiKey or add a fallback provider.");
        }

        List<Exception> failures = new();
        foreach (var provider in providers)
        {
            try
            {
                return await action(provider);
            }
            catch (Exception ex)
            {
                failures.Add(ex);
                _logger.LogWarning(ex, "AI provider {Provider} failed for {Operation}.", provider.Name, operationName);
            }
        }

        if (failures.Count == 1)
        {
            throw failures[0];
        }

        throw new AggregateException($"All AI providers failed for {operationName}.", failures);
    }

    private IEnumerable<ResolvedProvider> ResolveProviders()
    {
        var chain = new List<AiProviderConfig>
        {
            new()
            {
                Provider = _settings.Provider,
                ApiKey = _settings.ApiKey,
                BaseUrl = _settings.BaseUrl,
                Model = _settings.Model,
                MaxTokens = _settings.MaxTokens,
                Temperature = _settings.Temperature,
                TimeoutSeconds = _settings.TimeoutSeconds,
                Referer = _settings.Referer,
                SiteName = _settings.SiteName,
                SendProviderHeaders = _settings.SendProviderHeaders
            }
        };

        if (_settings.FallbackProviders is { Count: > 0 })
        {
            chain.AddRange(_settings.FallbackProviders);
        }

        foreach (var option in chain)
        {
            var apiKey = option.ApiKey ?? _settings.ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                continue;
            }

            var baseUrl = option.BaseUrl ?? _settings.BaseUrl ?? "https://api.openai.com/v1/";
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                _logger.LogWarning("Skipping AI provider {Provider} because base URL is invalid: {BaseUrl}", option.Provider ?? "Unknown", baseUrl);
                continue;
            }

            yield return new ResolvedProvider(
                Name: option.Provider ?? _settings.Provider ?? "OpenAI",
                BaseUri: baseUri,
                ApiKey: apiKey,
                Model: option.Model ?? _settings.Model ?? "gpt-3.5-turbo",
                MaxTokens: option.MaxTokens ?? _settings.MaxTokens ?? 600,
                Temperature: option.Temperature ?? _settings.Temperature ?? 0.7,
                SendProviderHeaders: option.SendProviderHeaders ?? _settings.SendProviderHeaders,
                Referer: option.Referer ?? _settings.Referer,
                SiteName: option.SiteName ?? _settings.SiteName,
                Timeout: TimeSpan.FromSeconds(Math.Max(5, option.TimeoutSeconds ?? _settings.TimeoutSeconds ?? 30))
            );
        }
    }

    private sealed record ResolvedProvider(
        string Name,
        Uri BaseUri,
        string ApiKey,
        string Model,
        int MaxTokens,
        double Temperature,
        bool SendProviderHeaders,
        string? Referer,
        string? SiteName,
        TimeSpan Timeout);

    private GrammarCheckResponse ParseGrammarResponse(string body)
    {
        using var doc = JsonDocument.Parse(body);
        var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;

        var response = new GrammarCheckResponse { Success = true };
        var issues = new List<GrammarIssue>();

        // Parse corrected text
        var correctedMatch = System.Text.RegularExpressions.Regex.Match(content, @"CORRECTED:\s*\n(.*?)(?=\nISSUES:|$)", System.Text.RegularExpressions.RegexOptions.Singleline);
        if (correctedMatch.Success)
        {
            response.CorrectedText = correctedMatch.Groups[1].Value.Trim();
        }

        // Parse issues
        var issuesMatch = System.Text.RegularExpressions.Regex.Match(content, @"ISSUES:\s*\n(.*)$", System.Text.RegularExpressions.RegexOptions.Singleline);
        if (issuesMatch.Success)
        {
            var issuesText = issuesMatch.Groups[1].Value.Trim();
            if (!issuesText.Contains("No issues found", StringComparison.OrdinalIgnoreCase))
            {
                var lines = issuesText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 5)
                    {
                        issues.Add(new GrammarIssue
                        {
                            Type = parts[0].Trim().ToLower(),
                            Severity = parts[1].Trim().ToLower(),
                            Original = parts[2].Trim(),
                            Suggestion = parts[3].Trim(),
                            Explanation = parts[4].Trim()
                        });
                    }
                }
            }
        }

        response.Issues = issues;
        response.Summary = issues.Count == 0 
            ? "Great! Your text looks good with no significant issues found." 
            : $"Found {issues.Count} issue(s): {issues.Count(i => i.Severity == "high")} high, {issues.Count(i => i.Severity == "medium")} medium, {issues.Count(i => i.Severity == "low")} low priority.";

        return response;
    }

    private Task<AiTextResponse> SendChatAsync(string mode, string userPrompt, string tone, string length)
    {
        return ExecuteWithProvidersAsync(
            provider => SendChatWithProviderAsync(provider, mode, userPrompt, tone, length),
            $"chat::{mode}");
    }

    private AiTextResponse ParseResponse(string mode, string tone, string length, string body)
    {
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        var content = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
        var usage = root.TryGetProperty("usage", out var usageProp)
            ? usageProp
            : default;

        var tokenEstimate = usage.ValueKind != JsonValueKind.Undefined && usage.TryGetProperty("total_tokens", out var total)
            ? total.GetInt32()
            : Math.Max(50, content.Length / 4);

        return new AiTextResponse
        {
            Mode = mode,
            Tone = tone,
            Length = length,
            Content = content.Trim(),
            EstimatedTokens = tokenEstimate,
            Tips = new List<string>
            {
                "Review the output for factual accuracy.",
                "Adjust tone/length and regenerate if it feels off.",
                "For best results, add context about audience and goal."
            }
        };
    }

}
