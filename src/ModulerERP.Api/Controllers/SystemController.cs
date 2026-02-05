using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModulerERP.SystemCore.Infrastructure.Persistence;

namespace ModulerERP.Api.Controllers;

/// <summary>
/// Health check and system info endpoints.
/// </summary>
[Route("api")]
public class SystemController : BaseApiController
{
    private readonly SystemCoreDbContext _context;

    public SystemController(SystemCoreDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        try
        {
            // Check database connectivity
            await _context.Database.CanConnectAsync();
            
            return Ok(new
            {
                status = "healthy",
                database = "connected",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "unhealthy",
                database = "disconnected",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// System info endpoint.
    /// </summary>
    [HttpGet("info")]
    public IActionResult Info()
    {
        return Ok(new
        {
            name = "ModulerERP",
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            runtime = Environment.Version.ToString()
        });
    }

    /// <summary>
    /// Get tenant summary (for dashboard).
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var currencyCount = await _context.Currencies.CountAsync();
        var languageCount = await _context.Languages.CountAsync();
        var userCount = await _context.Users.IgnoreQueryFilters().CountAsync();
        var roleCount = await _context.Roles.IgnoreQueryFilters().CountAsync();

        return Ok(new
        {
            success = true,
            data = new
            {
                currencies = currencyCount,
                languages = languageCount,
                users = userCount,
                roles = roleCount
            }
        });
    }
}
