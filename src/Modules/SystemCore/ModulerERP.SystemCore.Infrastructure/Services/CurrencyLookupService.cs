using Microsoft.EntityFrameworkCore;
using ModulerERP.SharedKernel.DTOs;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using ModulerERP.SystemCore.Infrastructure.Persistence;

namespace ModulerERP.SystemCore.Infrastructure.Services;

public class CurrencyLookupService : ICurrencyLookupService
{
    private readonly SystemCoreDbContext _context;

    public CurrencyLookupService(SystemCoreDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CurrencyLookupDto>>> GetActiveCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var currencies = await _context.Currencies
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Select(c => new CurrencyLookupDto(c.Id, c.Code, c.Symbol, c.IsActive))
                .ToListAsync(cancellationToken);

            return Result<List<CurrencyLookupDto>>.Success(currencies);
        }
        catch (Exception ex)
        {
            return Result<List<CurrencyLookupDto>>.Failure($"Failed to fetch currencies: {ex.Message}");
        }
    }
}
