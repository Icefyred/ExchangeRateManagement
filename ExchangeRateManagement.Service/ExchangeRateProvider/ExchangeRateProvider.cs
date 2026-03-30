using ExchangeRateManagement.Domain.ExchangeRate;
using ExchangeRateManagement.Service.Abstractions;
using System.Text.Json;

namespace ExchangeRateManagement.Service.ExchangeRateProvider;

public class ExchangeRateProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;

    public ExchangeRateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient; 
    }
    public async Task<ExchangeRate?> GetRateAsync(string baseCurrency, string targetCurrency)
    {
        baseCurrency = baseCurrency.ToUpperInvariant();
        targetCurrency = targetCurrency.ToUpperInvariant();

        var url = $"https://open.er-api.com/v6/latest/{baseCurrency}";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();

        using var json = JsonDocument.Parse(content);

        var rates = json.RootElement.GetProperty("rates");

        if (!rates.TryGetProperty(targetCurrency, out var rateElement))
            return null;

        var rate = rateElement.GetDecimal();

        return new ExchangeRate
        {
            BaseCurrency = baseCurrency,
            TargetCurrency = targetCurrency,
            Bid = rate,
            Ask = rate,
            LastUpdatedAt = DateTime.UtcNow
        };
    }
}
