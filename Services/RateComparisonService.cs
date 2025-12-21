using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Text.Json.Serialization;
using NovaToolsHub.Models.ViewModels;

namespace NovaToolsHub.Services;

public class RateComparisonService : IRateComparisonService
{
    private readonly ICurrencyService _currencyService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RateComparisonService> _logger;

    // Hardcoded mock historical deltas (simulated 24h/7d changes) used as last-resort fallback
    private static readonly Dictionary<string, (decimal? Change24h, decimal? Change7d)> MockDeltas = new()
    {
        { "USD", (0.0m, 0.0m) },
        { "EUR", (-0.12m, 0.35m) },
        { "GBP", (0.08m, -0.22m) },
        { "JPY", (0.45m, 1.10m) },
        { "CAD", (-0.05m, 0.18m) },
        { "AUD", (0.22m, -0.40m) },
        { "INR", (-0.30m, 0.55m) },
        { "CNY", (0.02m, -0.10m) },
        { "CHF", (-0.07m, 0.12m) },
        { "NZD", (0.15m, -0.25m) },
        { "SGD", (0.03m, 0.08m) },
        { "HKD", (0.00m, 0.02m) },
        { "SEK", (-0.18m, 0.42m) },
        { "NOK", (0.25m, -0.15m) },
        { "MXN", (-0.55m, 1.20m) },
        { "ZAR", (0.80m, -0.90m) },
        { "BRL", (-0.65m, 1.50m) },
        { "KRW", (0.10m, 0.30m) },
        { "RUB", (1.20m, -2.10m) },
        { "TRY", (-1.80m, 3.50m) }
    };

    public RateComparisonService(
        ICurrencyService currencyService,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<RateComparisonService> logger)
    {
        _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RateComparisonResponse> CompareAsync(RateComparisonRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Targets == null || request.Targets.Count == 0)
        {
            throw new InvalidOperationException("Add at least one currency to compare.");
        }

        var baseCurrency = request.BaseCurrency?.ToUpperInvariant() ?? "USD";
        var usedFallback = false;
        var rows = new List<RateComparisonRow>();
        Dictionary<string, (decimal? Change24h, decimal? Change7d)> deltas;
        var usedHistoricalFallback = false;

        Dictionary<string, decimal> rates;
        try
        {
            rates = await _currencyService.GetExchangeRatesAsync(baseCurrency);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Live rates unavailable; using fallback for {Base}", baseCurrency);
            rates = GetFallbackRates();
            usedFallback = true;
        }

        try
        {
            var historical = await GetHistoricalDeltasAsync(baseCurrency, rates, cancellationToken);
            deltas = historical.Deltas;
            usedHistoricalFallback = historical.UsedFallback;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Historical rate deltas unavailable; using mock deltas for {Base}", baseCurrency);
            deltas = MockDeltas;
            usedHistoricalFallback = true;
        }

        foreach (var target in request.Targets)
        {
            var currency = target.Currency?.ToUpperInvariant() ?? "EUR";
            var amount = Math.Max(0.0001m, target.Amount);

            decimal rate;
            if (currency == baseCurrency)
            {
                rate = 1m;
            }
            else if (rates.TryGetValue(currency, out var fetchedRate))
            {
                rate = fetchedRate;
            }
            else
            {
                _logger.LogWarning("Rate for {Currency} not found; using fallback", currency);
                rate = GetFallbackRates().GetValueOrDefault(currency, 1m);
                usedFallback = true;
            }

            var converted = amount * rate;
            var delta = deltas.GetValueOrDefault(currency, (null, null));

            rows.Add(new RateComparisonRow
            {
                Currency = currency,
                Rate = Math.Round(rate, 6),
                ConvertedAmount = Math.Round(converted, 4),
                Change24h = delta.Change24h.HasValue ? Math.Round(delta.Change24h.Value, 2) : null,
                Change7d = delta.Change7d.HasValue ? Math.Round(delta.Change7d.Value, 2) : null
            });
        }

        return new RateComparisonResponse
        {
            BaseCurrency = baseCurrency,
            RetrievedAtUtc = DateTime.UtcNow,
            Rows = rows,
            UsedFallbackRates = usedFallback,
            UsedHistoricalFallback = usedHistoricalFallback
        };
    }

    private async Task<(Dictionary<string, (decimal? Change24h, decimal? Change7d)> Deltas, bool UsedFallback)> GetHistoricalDeltasAsync(
        string baseCurrency,
        Dictionary<string, decimal> currentRates,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"hist_deltas_{baseCurrency}_{DateTime.UtcNow:yyyyMMdd}";
        if (_cache.TryGetValue(cacheKey, out Dictionary<string, (decimal? Change24h, decimal? Change7d)>? cached) && cached != null)
        {
            return (cached, false);
        }

        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-7);

        // Provider 1: Frankfurter (no API key)
        var frankfurter = await TryFrankfurterAsync(baseCurrency, startDate, endDate, currentRates, cancellationToken);
        if (frankfurter != null)
        {
            _cache.Set(cacheKey, frankfurter.Value.Deltas, TimeSpan.FromHours(3));
            return frankfurter.Value;
        }

        // Provider 2: CurrencyFreaks timeseries (API key)
        var cfKey = _configuration["ApiSettings:CurrencyFreaks:ApiKey"];
        if (!string.IsNullOrWhiteSpace(cfKey))
        {
            var currencyFreaks = await TryCurrencyFreaksAsync(baseCurrency, startDate, endDate, currentRates, cfKey, cancellationToken);
            if (currencyFreaks != null)
            {
                _cache.Set(cacheKey, currencyFreaks.Value.Deltas, TimeSpan.FromHours(3));
                return currencyFreaks.Value;
            }
        }

        _logger.LogWarning("All historical rate providers failed; using mock deltas for {Base}", baseCurrency);
        return (MockDeltas, true);
    }

    private static decimal CalculatePercentChange(decimal current, decimal previous)
    {
        if (previous == 0) return 0;
        return ((current - previous) / previous) * 100m;
    }

    private async Task<(Dictionary<string, (decimal? Change24h, decimal? Change7d)> Deltas, bool UsedFallback)?> TryFrankfurterAsync(
        string baseCurrency,
        DateTime startDate,
        DateTime endDate,
        Dictionary<string, decimal> currentRates,
        CancellationToken cancellationToken)
    {
        var url = $"https://api.frankfurter.app/{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?from={baseCurrency}";
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(10);

        var response = await client.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Frankfurter timeseries returned {Status} for {Base}", response.StatusCode, baseCurrency);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var payload = JsonSerializer.Deserialize<FrankfurterTimeSeriesResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (payload?.Rates == null || payload.Rates.Count == 0)
        {
            _logger.LogWarning("Frankfurter timeseries returned empty rates for {Base}", baseCurrency);
            return null;
        }

        return BuildDeltaMap(currentRates, payload.Rates, endDate);
    }

    private async Task<(Dictionary<string, (decimal? Change24h, decimal? Change7d)> Deltas, bool UsedFallback)?> TryCurrencyFreaksAsync(
        string baseCurrency,
        DateTime startDate,
        DateTime endDate,
        Dictionary<string, decimal> currentRates,
        string apiKey,
        CancellationToken cancellationToken)
    {
        var url = $"https://api.currencyfreaks.com/v2.0/timeseries?apikey={apiKey}&base={baseCurrency}&start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}";
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(10);

        var response = await client.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("CurrencyFreaks timeseries returned {Status} for {Base}", response.StatusCode, baseCurrency);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var payload = JsonSerializer.Deserialize<CurrencyFreaksTimeSeriesResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (payload?.Rates == null || payload.Rates.Count == 0)
        {
            _logger.LogWarning("CurrencyFreaks timeseries returned empty rates for {Base}", baseCurrency);
            return null;
        }

        return BuildDeltaMap(currentRates, payload.Rates, endDate);
    }

    private (Dictionary<string, (decimal? Change24h, decimal? Change7d)> Deltas, bool UsedFallback) BuildDeltaMap(
        Dictionary<string, decimal> currentRates,
        Dictionary<string, Dictionary<string, decimal>> historicalRates,
        DateTime endDate)
    {
        var day1Key = endDate.AddDays(-1).ToString("yyyy-MM-dd");
        var day7Key = endDate.AddDays(-7).ToString("yyyy-MM-dd");

        historicalRates.TryGetValue(day1Key, out var day1Rates);
        historicalRates.TryGetValue(day7Key, out var day7Rates);

        var result = new Dictionary<string, (decimal? Change24h, decimal? Change7d)>();
        var usedFallback = false;

        foreach (var kvp in currentRates)
        {
            var currency = kvp.Key;
            var currentRate = kvp.Value;

            decimal? change24h = null;
            decimal? change7d = null;

            if (day1Rates != null && day1Rates.TryGetValue(currency, out var rate1) && rate1 > 0)
            {
                change24h = CalculatePercentChange(currentRate, rate1);
            }
            else if (MockDeltas.TryGetValue(currency, out var mock1))
            {
                change24h = mock1.Change24h;
                usedFallback = true;
            }

            if (day7Rates != null && day7Rates.TryGetValue(currency, out var rate7) && rate7 > 0)
            {
                change7d = CalculatePercentChange(currentRate, rate7);
            }
            else if (MockDeltas.TryGetValue(currency, out var mock7))
            {
                change7d = mock7.Change7d;
                usedFallback = true;
            }

            result[currency] = (change24h, change7d);
        }

        return (result, usedFallback);
    }

    private class FrankfurterTimeSeriesResponse
    {
        [JsonPropertyName("rates")]
        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; } = new();
    }

    private class CurrencyFreaksTimeSeriesResponse
    {
        [JsonPropertyName("rates")]
        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; } = new();
    }

    private static Dictionary<string, decimal> GetFallbackRates()
    {
        return new Dictionary<string, decimal>
        {
            { "USD", 1.0m },
            { "EUR", 0.92m },
            { "GBP", 0.79m },
            { "JPY", 149.50m },
            { "CAD", 1.36m },
            { "AUD", 1.53m },
            { "INR", 83.20m },
            { "CNY", 7.24m },
            { "CHF", 0.88m },
            { "NZD", 1.64m },
            { "SGD", 1.34m },
            { "HKD", 7.82m },
            { "SEK", 10.45m },
            { "NOK", 10.85m },
            { "MXN", 17.15m },
            { "ZAR", 18.60m },
            { "BRL", 4.97m },
            { "KRW", 1320m },
            { "RUB", 92.50m },
            { "TRY", 28.80m }
        };
    }
}
