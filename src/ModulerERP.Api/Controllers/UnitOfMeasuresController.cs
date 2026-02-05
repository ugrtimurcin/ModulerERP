using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;
using ModulerERP.Inventory.Domain.Enums;

namespace ModulerERP.Api.Controllers;

//[Authorize]
[Route("api/unit-of-measures")]
public class UnitOfMeasuresController : BaseApiController
{
    private readonly IUnitOfMeasureService _service;

    public UnitOfMeasuresController(IUnitOfMeasureService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UnitOfMeasureDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(TenantId, cancellationToken);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<UnitOfMeasureDto>>> GetByType(UomType type, CancellationToken cancellationToken)
    {
        var result = await _service.GetByTypeAsync(type, TenantId, cancellationToken);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UnitOfMeasureDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, TenantId, cancellationToken);
        if (result == null)
            return NotFound(new { success = false, message = "UOM not found" });
        
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<ActionResult<UnitOfMeasureDto>> Create(CreateUnitOfMeasureDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateAsync(dto, TenantId, CurrentUserId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateUnitOfMeasureDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _service.UpdateAsync(id, dto, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, message = "UOM updated successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "UOM not found" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(id, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, message = "UOM deleted successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "UOM not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
