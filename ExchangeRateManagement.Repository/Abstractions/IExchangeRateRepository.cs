using ExchangeRateManagement.Domain.ExchangeRate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateManagement.Repository.Abstractions;

public interface IExchangeRateRepository
{
    Task<ExchangeRate?> GetByCurrencyPairAsync(string baseCurrency, string targetCurrency);

    Task<ExchangeRate?> GetByIdAsync(Guid id);

    Task AddAsync(ExchangeRate rate);

    Task UpdateAsync(ExchangeRate rate);

    Task DeleteAsync(Guid id);
}
