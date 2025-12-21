namespace NovaToolsHub.Services;

using Microsoft.Extensions.Caching.Memory;
using System.Text.Json.Serialization;

/// <summary>
/// Response model for CurrencyFreaks API
/// </summary>
public class CurrencyFreaksResponse
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }
    
    [JsonPropertyName("base")]
    public string? Base { get; set; }
    
    [JsonPropertyName("rates")]
    public Dictionary<string, string>? Rates { get; set; }
}

/// <summary>
/// Response model for ExchangeRate-API (fallback)
/// </summary>
public class ExchangeRateApiResponse
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }
    
    [JsonPropertyName("base_code")]
    public string? BaseCode { get; set; }
    
    [JsonPropertyName("base")]
    public string? Base { get; set; }
    
    [JsonPropertyName("date")]
    public string? Date { get; set; }
    
    // v6 API uses "conversion_rates"
    [JsonPropertyName("conversion_rates")]
    public Dictionary<string, double>? ConversionRates { get; set; }
    
    // Open API uses "rates"
    [JsonPropertyName("rates")]
    public Dictionary<string, double>? Rates { get; set; }
    
    // Helper to get whichever rates property is populated
    public Dictionary<string, double>? GetRates() => ConversionRates ?? Rates;
}

/// <summary>
/// Service for currency conversion operations
/// Primary: CurrencyFreaks API | Fallback: ExchangeRate-API
/// </summary>
public interface ICurrencyService
{
    Task<decimal> ConvertAsync(string fromCurrency, string toCurrency, decimal amount);
    Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string baseCurrency);
    Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
}

public class CurrencyService : ICurrencyService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CurrencyService> _logger;
    private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public CurrencyService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<CurrencyService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<decimal> ConvertAsync(string fromCurrency, string toCurrency, decimal amount)
    {
        try
        {
            var rates = await GetExchangeRatesAsync(fromCurrency);
            if (rates.ContainsKey(toCurrency))
            {
                return amount * rates[toCurrency];
            }
            throw new Exception($"Exchange rate for {toCurrency} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting currency from {From} to {To}", fromCurrency, toCurrency);
            // Fallback to mock data
            var mockRates = GetMockRates(fromCurrency);
            if (mockRates.ContainsKey(toCurrency))
                return amount * mockRates[toCurrency];
            return amount * 1.0m;
        }
    }

    public async Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string baseCurrency)
    {
        var cacheKey = $"rates_{baseCurrency.ToUpper()}";
        
        // Check cache first
        if (_cache.TryGetValue(cacheKey, out Dictionary<string, decimal>? cachedRates) && cachedRates != null)
        {
            _logger.LogDebug("Returning cached rates for {Base}", baseCurrency);
            return cachedRates;
        }

        // Try CurrencyFreaks API first (primary)
        var currencyFreaksKey = _configuration["ApiSettings:CurrencyFreaks:ApiKey"];
        if (!string.IsNullOrEmpty(currencyFreaksKey))
        {
            var rates = await TryGetFromCurrencyFreaks(baseCurrency, currencyFreaksKey, cacheKey);
            if (rates != null) return rates;
        }

        // Fallback to ExchangeRate-API
        var exchangeRateApiKey = _configuration["ApiSettings:CurrencyApi:ApiKey"];
        var rates2 = await TryGetFromExchangeRateApi(baseCurrency, exchangeRateApiKey, cacheKey);
        if (rates2 != null) return rates2;

        // Final fallback to mock data
        _logger.LogWarning("All currency APIs failed, using mock data for {Base}", baseCurrency);
        return GetMockRates(baseCurrency);
    }

    private async Task<Dictionary<string, decimal>?> TryGetFromCurrencyFreaks(string baseCurrency, string apiKey, string cacheKey)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            // CurrencyFreaks always returns USD-based rates, so we fetch USD and convert
            var requestUrl = $"https://api.currencyfreaks.com/v2.0/rates/latest?apikey={apiKey}";
            _logger.LogInformation("Fetching rates from CurrencyFreaks API");

            var response = await httpClient.GetAsync(requestUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonSerializer.Deserialize<CurrencyFreaksResponse>(json);
                
                if (data?.Rates != null)
                {
                    // Parse string rates to decimal
                    var usdRates = new Dictionary<string, decimal>();
                    foreach (var kvp in data.Rates)
                    {
                        if (decimal.TryParse(kvp.Value, System.Globalization.NumberStyles.Any, 
                            System.Globalization.CultureInfo.InvariantCulture, out var rate))
                        {
                            usdRates[kvp.Key] = rate;
                        }
                    }
                    usdRates["USD"] = 1.0m; // Ensure USD is present
                    
                    // Convert to requested base currency
                    var rates = ConvertToBase(usdRates, baseCurrency.ToUpper());
                    
                    // Cache the result
                    _cache.Set(cacheKey, rates, _cacheExpiration);
                    _logger.LogInformation("Successfully fetched {Count} rates from CurrencyFreaks for {Base}", rates.Count, baseCurrency);
                    return rates;
                }
            }
            
            _logger.LogWarning("CurrencyFreaks API returned status {Status}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from CurrencyFreaks API");
        }
        
        return null;
    }

    private async Task<Dictionary<string, decimal>?> TryGetFromExchangeRateApi(string baseCurrency, string? apiKey, string cacheKey)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            string requestUrl;
            
            // If API key is provided, use the v6 authenticated endpoint
            if (!string.IsNullOrEmpty(apiKey))
            {
                requestUrl = $"https://v6.exchangerate-api.com/v6/{apiKey}/latest/{baseCurrency.ToUpper()}";
            }
            else
            {
                // Use the free open API (no key required)
                requestUrl = $"https://open.er-api.com/v6/latest/{baseCurrency.ToUpper()}";
            }
            
            _logger.LogInformation("Fetching rates from ExchangeRate-API (fallback)");

            var response = await httpClient.GetAsync(requestUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonSerializer.Deserialize<ExchangeRateApiResponse>(json);
                
                var apiRates = data?.GetRates();
                if (apiRates != null)
                {
                    var rates = apiRates.ToDictionary(
                        kvp => kvp.Key,
                        kvp => (decimal)kvp.Value
                    );
                    
                    // Cache the result
                    _cache.Set(cacheKey, rates, _cacheExpiration);
                    _logger.LogInformation("Successfully fetched {Count} rates from ExchangeRate-API for {Base}", rates.Count, baseCurrency);
                    return rates;
                }
            }
            
            _logger.LogWarning("ExchangeRate-API returned status {Status}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from ExchangeRate-API");
        }
        
        return null;
    }

    private Dictionary<string, decimal> ConvertToBase(Dictionary<string, decimal> usdRates, string baseCurrency)
    {
        // If base is USD, return as-is
        if (baseCurrency == "USD")
            return usdRates;
        
        // Get the rate for the new base currency (relative to USD)
        if (!usdRates.TryGetValue(baseCurrency, out var baseRate) || baseRate == 0)
        {
            _logger.LogWarning("Base currency {Base} not found in rates, using USD", baseCurrency);
            return usdRates;
        }
        
        // Convert all rates to the new base
        var convertedRates = new Dictionary<string, decimal>();
        foreach (var kvp in usdRates)
        {
            convertedRates[kvp.Key] = kvp.Value / baseRate;
        }
        
        return convertedRates;
    }

    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        try
        {
            var rates = await GetExchangeRatesAsync(fromCurrency);
            
            // If same currency, return 1
            if (fromCurrency == toCurrency)
                return 1.0m;
            
            // Get rate from base currency
            if (rates.ContainsKey(toCurrency))
                return rates[toCurrency];
            
            // If not found, try inverse lookup
            var inverseRates = await GetExchangeRatesAsync(toCurrency);
            if (inverseRates.ContainsKey(fromCurrency))
                return 1.0m / inverseRates[fromCurrency];
            
            throw new Exception($"Exchange rate for {toCurrency} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exchange rate from {From} to {To}", fromCurrency, toCurrency);
            // Return mock rate for development
            return 1.2m;
        }
    }

    private Dictionary<string, decimal> GetMockRates(string baseCurrency)
    {
        // Mock exchange rates for development
        return new Dictionary<string, decimal>
        {
            { "USD", 1.0m },
            { "EUR", 0.85m },
            { "GBP", 0.73m },
            { "JPY", 110.0m },
            { "CAD", 1.25m },
            { "AUD", 1.35m },
            { "INR", 74.5m },
            { "CNY", 6.45m }
        };
    }
}
