using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModulerERP.SystemCore.Infrastructure.Persistence;

namespace ModulerERP.Api.Controllers;

/// <summary>
/// Currency management endpoints.
/// </summary>
public class CurrenciesController : BaseApiController
{
    private readonly SystemCoreDbContext _context;

    public CurrenciesController(SystemCoreDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all currencies.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var currencies = await _context.Currencies
            .AsNoTracking()
            .Select(c => new
            {
                c.Id,
                c.Code,
                c.Name,
                c.Symbol,
                c.Precision,
                c.IsActive
            })
            .ToListAsync();

        return Ok(new { success = true, data = currencies });
    }

    /// <summary>
    /// Get currency by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var currency = await _context.Currencies
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Code,
                c.Name,
                c.Symbol,
                c.Precision,
                c.IsActive
            })
            .FirstOrDefaultAsync();

        if (currency == null)
            return NotFound(new { error = "Currency not found" });

        return Ok(new { success = true, data = currency });
    }

    /// <summary>
    /// Get active currencies.
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var currencies = await _context.Currencies
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new
            {
                c.Id,
                c.Code,
                c.Name,
                c.Symbol
            })
            .ToListAsync();

        return Ok(new { success = true, data = currencies });
    }
}
