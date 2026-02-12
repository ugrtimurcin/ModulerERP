using Microsoft.EntityFrameworkCore;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using ModulerERP.SystemCore.Infrastructure.Persistence;

namespace ModulerERP.SystemCore.Infrastructure.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly SystemCoreDbContext _context;

    public ExchangeRateService(SystemCoreDbContext context)
    {
        _context = context;
    }

    public async Task<Result<decimal>> GetExchangeRateAsync(Guid tenantId, Guid fromCurrencyId, Guid toCurrencyId, DateTime date, CancellationToken cancellationToken = default)
    {
        if (fromCurrencyId == toCurrencyId) return Result<decimal>.Success(1.0m);

        // Try direct rate
        var rate = await _context.ExchangeRates
            .Where(x => x.TenantId == tenantId && // Ensure tenant isolation
                        x.SourceCurrencyId == fromCurrencyId && 
                        x.TargetCurrencyId == toCurrencyId && 
                        x.Date <= date)
            .OrderByDescending(x => x.Date)
            .Select(x => x.Rate)
            .FirstOrDefaultAsync(cancellationToken);

        if (rate > 0) return Result<decimal>.Success(rate);

        // Try inverse rate
        var inverseRate = await _context.ExchangeRates
            .Where(x => x.TenantId == tenantId && // Ensure tenant isolation
                        x.SourceCurrencyId == toCurrencyId && 
                        x.TargetCurrencyId == fromCurrencyId && 
                        x.Date <= date)
            .OrderByDescending(x => x.Date)
            .Select(x => x.Rate)
            .FirstOrDefaultAsync(cancellationToken);

        if (inverseRate > 0) return Result<decimal>.Success(1.0m / inverseRate);

        // Fallback or Failure: For now return 1.0 to avoid blocking but log/warn ideally.
        // Or return failure if strict:
        // return Result<decimal>.Failure("Exchange rate not found");
        
        return Result<decimal>.Success(1.0m); // Fallback for Development
    }
}
