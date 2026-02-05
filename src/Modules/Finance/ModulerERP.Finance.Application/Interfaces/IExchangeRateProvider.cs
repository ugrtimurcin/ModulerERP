using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Finance.Application.Interfaces;

public interface IExchangeRateProvider
{
    /// <summary>
    /// Fetches the latest daily exchange rates from the provider.
    /// Returns a list of (CurrencyCode, Rate).
    /// </summary>
    Task<Result<List<(string CurrencyCode, decimal Rate)>>> GetDailyRatesAsync(DateTime? date = null, CancellationToken cancellationToken = default);
}
