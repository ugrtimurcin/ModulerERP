using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Finance.Application.Interfaces;

public interface IExchangeRateProvider
{
    /// <summary>
    /// Fetches the latest daily exchange rates from the provider.
    /// Returns a list of ExternalRateDto.
    /// </summary>
    Task<Result<List<ModulerERP.Finance.Application.DTOs.ExternalRateDto>>> GetDailyRatesAsync(DateTime? date = null, CancellationToken cancellationToken = default);
}
