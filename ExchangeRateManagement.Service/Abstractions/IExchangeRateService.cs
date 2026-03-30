using ExchangeRateManagement.Domain.ExchangeRate;

namespace ExchangeRateManagement.Service.Abstractions
{
    public interface IExchangeRateService
    {
        Task<ExchangeRate?> GetRateAsync(string baseCurrency, string targetCurrency);

        Task CreateRateAsync(ExchangeRate rate);

        Task UpdateRateAsync(ExchangeRate rate);

        Task DeleteRateAsync(string baseCurrency, string targetCurrency);
    }
}
