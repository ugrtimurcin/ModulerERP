using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModulerERP.SystemCore.Infrastructure.Persistence;

namespace ModulerERP.Api.Controllers;

/// <summary>
/// Language management endpoints.
/// </summary>
public class LanguagesController : BaseApiController
{
    private readonly SystemCoreDbContext _context;

    public LanguagesController(SystemCoreDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all languages.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var languages = await _context.Languages
            .AsNoTracking()
            .Select(l => new
            {
                l.Id,
                l.Code,
                l.Name,
                l.IsRtl,
                l.IsActive
            })
            .ToListAsync();

        return Ok(new { success = true, data = languages });
    }

    /// <summary>
    /// Get active languages.
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var languages = await _context.Languages
            .AsNoTracking()
            .Where(l => l.IsActive)
            .Select(l => new
            {
                l.Id,
                l.Code,
                l.Name,
                l.IsRtl
            })
            .ToListAsync();

        return Ok(new { success = true, data = languages });
    }
}
