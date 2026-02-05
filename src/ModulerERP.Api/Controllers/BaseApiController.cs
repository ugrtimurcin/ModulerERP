using Microsoft.AspNetCore.Mvc;

namespace ModulerERP.Api.Controllers;

/// <summary>
/// Base controller with common functionality.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Current tenant ID from claims (to be populated by middleware)
    /// </summary>
    protected Guid TenantId => GetTenantId();

    /// <summary>
    /// Current user ID from claims (to be populated by middleware)
    /// </summary>
    protected Guid CurrentUserId => GetCurrentUserId();

    private Guid GetTenantId()
    {
        // TODO: Get from JWT claims
        var claim = User.FindFirst("tenant_id");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) 
            ? tenantId 
            : Guid.Empty;
    }

    private Guid GetCurrentUserId()
    {
        // TODO: Get from JWT claims
        var claim = User.FindFirst("sub") ?? User.FindFirst("user_id");
        return claim != null && Guid.TryParse(claim.Value, out var userId) 
            ? userId 
            : Guid.Empty;
    }

    /// <summary>
    /// Standard OK response with data
    /// </summary>
    protected ActionResult<T> OkResult<T>(T data) => Ok(new { success = true, data });

    /// <summary>
    /// Standard error response
    /// </summary>
    protected ActionResult ErrorResult(string message, int statusCode = 400)
    {
        return StatusCode(statusCode, new { success = false, error = message });
    }

    /// <summary>
    /// Handles Result pattern response
    /// </summary>
    protected ActionResult<T> HandleResult<T>(ModulerERP.SharedKernel.Results.Result<T> result)
    {
        if (result == null) return NotFound();
        if (result.IsSuccess && result.Value != null) return OkResult(result.Value);
        if (result.IsSuccess && result.Value == null) return NotFound();
        return ErrorResult(result.Error ?? "Unknown error");
    }

    protected ActionResult HandleResult(ModulerERP.SharedKernel.Results.Result result)
    {
        if (result == null) return NotFound();
        if (result.IsSuccess) return Ok(new { success = true });
        return ErrorResult(result.Error ?? "Unknown error");
    }
}
