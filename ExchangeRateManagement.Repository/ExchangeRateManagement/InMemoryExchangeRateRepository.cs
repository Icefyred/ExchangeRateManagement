using ExchangeRateManagement.Domain.ExchangeRate;
using ExchangeRateManagement.Repository.Abstractions;
using System.Collections.Concurrent;
using System.Linq;

namespace ExchangeRateManagement.Repository.ExchangeRateManagement;

public class InMemoryExchangeRateRepository : IExchangeRateRepository
{
    private readonly ConcurrentDictionary<Guid, ExchangeRate> _store = new();

    public Task AddAsync(ExchangeRate rate)
    {
        if (rate.Id == Guid.Empty)
            rate.Id = Guid.NewGuid();

        //normalize the input to ensure consistent comparisons
        rate.BaseCurrency = rate.BaseCurrency.ToUpper();
        rate.TargetCurrency = rate.TargetCurrency.ToUpper();

        _store.TryAdd(rate.Id, rate);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<ExchangeRate?> GetByCurrencyPairAsync(string baseCurrency, string targetCurrency)
    {
        //normalize the input to ensure consistent comparisons
        baseCurrency = baseCurrency.ToUpper();
        targetCurrency = targetCurrency.ToUpper();

        //search the in-memory store for the first entry matching both base and target currencies.
        var entity = _store.Values.FirstOrDefault(x =>
            x.BaseCurrency == baseCurrency &&
            x.TargetCurrency == targetCurrency);

        return Task.FromResult(entity);
    }

    public Task<ExchangeRate?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var rate);
        return Task.FromResult(rate);
    }

    public Task UpdateAsync(ExchangeRate rate)
    {
        _store[rate.Id] = rate;
        return Task.CompletedTask;
    }
}
