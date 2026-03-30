using ExchangeRateManagement.Domain.ExchangeRate;
using ExchangeRateManagement.Repository.Abstractions;
using ExchangeRateManagement.Service.Abstractions;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExchangeRateManagement.Service.ExchangeRateService;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateRepository _repository;
    private readonly IExchangeRateProvider _provider;
    private readonly ILogger<ExchangeRateService> _logger;

    private const int CacheDurationMinutes = 30; 

    public ExchangeRateService(IExchangeRateRepository repository, IExchangeRateProvider provider, ILogger<ExchangeRateService> logger)
    {
        _repository = repository;
        _provider = provider;
        _logger = logger;
    }

    public async Task CreateRateAsync(ExchangeRate rate)
    {
        rate.BaseCurrency = rate.BaseCurrency.ToUpperInvariant();
        rate.TargetCurrency = rate.TargetCurrency.ToUpperInvariant();
        rate.LastUpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Creating exchange rate {Base}/{Target}", rate.BaseCurrency, rate.TargetCurrency);
        await _repository.AddAsync(rate);
    }

    public async Task DeleteRateAsync(string baseCurrency, string targetCurrency)
    {
        baseCurrency = baseCurrency.ToUpperInvariant();
        targetCurrency = targetCurrency.ToUpperInvariant();

        var existing = await _repository.GetByCurrencyPairAsync(baseCurrency, targetCurrency);

        if (existing != null)
        {
            _logger.LogInformation("Deleting exchange rate {Base}/{Target}", baseCurrency, targetCurrency);
            await _repository.DeleteAsync(existing.Id);
        }
        else
        {
            _logger.LogInformation("No exchange rate {Base}/{Target} found", baseCurrency, targetCurrency);
        }
    }

    public async Task<ExchangeRate?> GetRateAsync(string baseCurrency, string targetCurrency)
    {
        baseCurrency = baseCurrency.ToUpper();
        targetCurrency = targetCurrency.ToUpper();

        // Edge case, where an user inputs the same currency
        if (baseCurrency == targetCurrency)
        {
            return new ExchangeRate
            {
                BaseCurrency = baseCurrency,
                TargetCurrency = targetCurrency,
                Bid = 1,
                Ask = 1,
                LastUpdatedAt = DateTime.UtcNow
            };
        }

        var exchangeRate = await _repository.GetByCurrencyPairAsync(baseCurrency, targetCurrency);

        if (exchangeRate == null)
        {
            _logger.LogInformation("Cache miss for {Base}/{Target}. Fetching from external API.", baseCurrency, targetCurrency);
            var fetchedRate = await _provider.GetRateAsync(baseCurrency, targetCurrency);
            if (fetchedRate == null)
            {
                _logger.LogWarning("Failed to fetch rate from external API for {Base}/{Target}. Returning fallback.", baseCurrency, targetCurrency);
                return exchangeRate;
            }
            await _repository.AddAsync(fetchedRate); 
            return fetchedRate;
        }

        _logger.LogInformation("Cache hit for {Base}/{Target}. Returning stored value.", baseCurrency, targetCurrency);
        //we check if the data kept is stale or not
        var isStale = exchangeRate.LastUpdatedAt < DateTime.UtcNow.AddMinutes(-CacheDurationMinutes);
        if (isStale)
        {
            _logger.LogInformation("Cache stale for {Base}/{Target}. Refreshing from external API.", baseCurrency, targetCurrency);
            var fetchedRate = await _provider.GetRateAsync(baseCurrency, targetCurrency);
            if (fetchedRate == null)
            {
                _logger.LogWarning("Failed to fetch rate from external API for {Base}/{Target}. Returning fallback.", baseCurrency, targetCurrency);
                return exchangeRate;
            }
            await _repository.UpdateAsync(fetchedRate); 
            return fetchedRate;
        }
        else 
            return exchangeRate;

    }

    public async Task UpdateRateAsync(ExchangeRate rate)
    {
        rate.BaseCurrency = rate.BaseCurrency.ToUpperInvariant();
        rate.TargetCurrency = rate.TargetCurrency.ToUpperInvariant();
        rate.LastUpdatedAt = DateTime.UtcNow;
        _logger.LogInformation("Updating exchange rate {Base}/{Target}", rate.BaseCurrency, rate.TargetCurrency);
        await _repository.UpdateAsync(rate);
    }


}
