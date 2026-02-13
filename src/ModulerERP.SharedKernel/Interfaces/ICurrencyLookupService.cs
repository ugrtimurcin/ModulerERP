using ModulerERP.SharedKernel.DTOs;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.SharedKernel.Interfaces;

public interface ICurrencyLookupService
{
    Task<Result<List<CurrencyLookupDto>>> GetActiveCurrenciesAsync(CancellationToken cancellationToken = default);
}
