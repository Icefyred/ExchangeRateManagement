using ExchangeRateManagement.Domain.ExchangeRate;

namespace ExchangeRateManagement.Service.Abstractions;

public interface IExchangeRateProvider
{
    Task<ExchangeRate?> GetRateAsync(string baseCurrency, string targetCurrency);
}
