using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.SharedKernel.DTOs;
using ModulerERP.SystemCore.Application.Constants;
using ModulerERP.SystemCore.Application.DTOs;
using ModulerERP.SystemCore.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

//[Authorize] // We need a stronger policy here later, e.g. [Authorize(Roles = "SystemAdmin")]
public class TenantsController : BaseApiController
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TenantListDto>>> GetTenants([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (TenantId != SystemCoreConstants.SystemTenantId) return Forbid();
        var result = await _tenantService.GetTenantsAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TenantDto>> GetTenant(Guid id)
    {
        if (TenantId != SystemCoreConstants.SystemTenantId) return Forbid();
        var tenant = await _tenantService.GetTenantByIdAsync(id);
        if (tenant == null) return NotFound();
        return Ok(tenant);
    }

    [HttpPost]
    public async Task<ActionResult<TenantDto>> CreateTenant(CreateTenantDto dto)
    {
        if (TenantId != SystemCoreConstants.SystemTenantId) return Forbid();
        try
        {
            var tenant = await _tenantService.CreateTenantAsync(dto);
            return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, tenant);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TenantDto>> UpdateTenant(Guid id, UpdateTenantDto dto)
    {
        if (TenantId != SystemCoreConstants.SystemTenantId) return Forbid();
        try
        {
            var tenant = await _tenantService.UpdateTenantAsync(id, dto);
            return Ok(tenant);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
