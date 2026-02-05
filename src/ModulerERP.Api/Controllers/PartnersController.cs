using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

/// <summary>
/// Business Partners management endpoints.
/// </summary>
//[Authorize]
public class PartnersController : BaseApiController
{
    private readonly IBusinessPartnerService _partnerService;

    public PartnersController(IBusinessPartnerService partnerService)
    {
        _partnerService = partnerService;
    }

    /// <summary>
    /// Get all partners (paginated).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isCustomer = null,
        [FromQuery] bool? isSupplier = null)
    {
        var result = await _partnerService.GetPartnersAsync(TenantId, page, pageSize, isCustomer, isSupplier);
        return Ok(new { 
            success = true, 
            data = new { 
                data = result.Data, 
                pagination = new { result.Page, result.PageSize, result.TotalCount, result.TotalPages } 
            } 
        });
    }

    /// <summary>
    /// Get partner by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var partner = await _partnerService.GetPartnerByIdAsync(TenantId, id);
        if (partner == null)
            return NotFound(new { success = false, error = "Partner not found" });

        return Ok(new { success = true, data = partner });
    }

    /// <summary>
    /// Create new partner.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBusinessPartnerDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        try
        {
            var partner = await _partnerService.CreatePartnerAsync(TenantId, request, CurrentUserId);
            return CreatedAtAction(nameof(GetById), new { id = partner.Id }, new { success = true, data = partner });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Update partner.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBusinessPartnerDto request)
    {
        try
        {
            var partner = await _partnerService.UpdatePartnerAsync(TenantId, id, request);
            return Ok(new { success = true, data = partner });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, error = "Partner not found" });
        }
    }

    /// <summary>
    /// Delete partner (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _partnerService.DeletePartnerAsync(TenantId, id, CurrentUserId);
            return Ok(new { success = true, message = "Partner deleted" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, error = "Partner not found" });
        }
    }
}
