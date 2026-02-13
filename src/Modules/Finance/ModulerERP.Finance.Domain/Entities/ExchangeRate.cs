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

    /// <summary>Buying Rate</summary>
    public decimal BuyingRate { get; private set; }

    /// <summary>Selling Rate</summary>
    public decimal SellingRate { get; private set; }
    
    public ExchangeRateSource Source { get; private set; } = ExchangeRateSource.Manual;

    private ExchangeRate() { } // EF Core

    public static ExchangeRate Create(
        Guid tenantId,
        Guid fromCurrencyId,
        Guid toCurrencyId,
        DateTime rateDate,
        decimal rate,
        decimal buyingRate,
        decimal sellingRate,
        ExchangeRateSource source = ExchangeRateSource.Manual)
    {
        if (rate <= 0 && buyingRate <= 0 && sellingRate <= 0)
            throw new ArgumentException("At least one rate must be positive");

        var entity = new ExchangeRate
        {
            FromCurrencyId = fromCurrencyId,
            ToCurrencyId = toCurrencyId,
            RateDate = rateDate, // Keep full timestamp
            Rate = rate,
            BuyingRate = buyingRate,
            SellingRate = sellingRate,
            Source = source
        };
        entity.SetTenant(tenantId);
        
        return entity;
    }

    /// <summary>Convert amount using this rate</summary>
    public decimal Convert(decimal amount) => amount * Rate;
    
    /// <summary>Reverse convert amount</summary>
    public decimal ReverseConvert(decimal amount) => amount / Rate;

    public void UpdateRates(decimal rate, decimal buyingRate, decimal sellingRate)
    {
        if (rate <= 0 && buyingRate <= 0 && sellingRate <= 0) throw new ArgumentException("At least one rate must be positive");
        Rate = rate;
        BuyingRate = buyingRate;
        SellingRate = sellingRate;
    }
}
