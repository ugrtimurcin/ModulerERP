
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.SharedKernel.Interfaces;

public interface IExchangeRateService
{
    /// <summary>
    /// Gets the exchange rate from source currency to target currency for a specific date.
    /// Returns 1.0 if currencies are the same.
    /// </summary>
    Task<Result<decimal>> GetExchangeRateAsync(Guid tenantId, Guid fromCurrencyId, Guid toCurrencyId, DateTime date, CancellationToken cancellationToken = default);
}
