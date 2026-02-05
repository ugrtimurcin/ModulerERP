using System.Security.Claims;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId != null ? Guid.Parse(userId) : Guid.Empty;
        }
    }

    public Guid TenantId
    {
        get
        {
            var tenantId = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value;
            if (string.IsNullOrEmpty(tenantId))
            {
                // Fallback to header if claim is missing (sometimes useful for initial auth flows)
                if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-Tenant-ID", out var headerValue) == true)
                {
                    tenantId = headerValue.ToString();
                }
            }
            return tenantId != null ? Guid.Parse(tenantId) : Guid.Empty;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
