using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Finance.Domain.Entities;

/// <summary>
/// Exchange rate history.
/// TRNC Critical - daily rates for TRY/GBP/EUR/USD.
/// </summary>
public class ExchangeRate : BaseEntity
{
    /// <summary>Source currency</summary>
    public Guid FromCurrencyId { get; private set; }
    
    /// <summary>Target currency (usually base currency)</summary>
    public Guid ToCurrencyId { get; private set; }
    
    /// <summary>Rate date</summary>
    public DateTime RateDate { get; private set; }
    
    /// <summary>Exchange rate</summary>
    public decimal Rate { get; private set; }
    
    public ExchangeRateSource Source { get; private set; } = ExchangeRateSource.Manual;

    private ExchangeRate() { } // EF Core

    public static ExchangeRate Create(
        Guid tenantId,
        Guid fromCurrencyId,
        Guid toCurrencyId,
        DateTime rateDate,
        decimal rate,
        ExchangeRateSource source = ExchangeRateSource.Manual)
    {
        if (rate <= 0)
            throw new ArgumentException("Rate must be positive");

        var entity = new ExchangeRate
        {
            FromCurrencyId = fromCurrencyId,
            ToCurrencyId = toCurrencyId,
            RateDate = rateDate.Date,
            Rate = rate,
            Source = source
        };
        entity.SetTenant(tenantId);
        
        return entity;
    }

    /// <summary>Convert amount using this rate</summary>
    public decimal Convert(decimal amount) => amount * Rate;
    
    /// <summary>Reverse convert amount</summary>
    public decimal ReverseConvert(decimal amount) => amount / Rate;

    public void UpdateRate(decimal newRate)
    {
        if (newRate <= 0) throw new ArgumentException("Rate must be positive");
        Rate = newRate;
    }
}
