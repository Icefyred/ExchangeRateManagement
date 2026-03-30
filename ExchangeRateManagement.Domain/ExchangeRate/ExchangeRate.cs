namespace ExchangeRateManagement.Domain.ExchangeRate;

public class ExchangeRate
{
    //I introduced a surrogate Guid identifier for internal consistency, but I would expose the natural key (base/target currency pair)
    //in the API since it is more meaningful to the user.
    public Guid Id { get; set; }

    public string BaseCurrency { get; set; }  // e.g. USD
    public string TargetCurrency { get; set; } // e.g. EUR

    public decimal Bid { get; set; }
    public decimal Ask { get; set; }

    public DateTime LastUpdatedAt { get; set; }
}
