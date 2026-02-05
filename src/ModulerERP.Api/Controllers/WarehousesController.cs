using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

//[Authorize]
public class WarehousesController : BaseApiController
{
    private readonly IWarehouseService _service;

    public WarehousesController(IWarehouseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(TenantId, cancellationToken);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WarehouseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, TenantId, cancellationToken);
        if (result == null)
            return NotFound(new { success = false, message = "Warehouse not found" });
        
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseDto>> Create(CreateWarehouseDto dto, CancellationToken cancellationToken)
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
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateWarehouseDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _service.UpdateAsync(id, dto, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, message = "Warehouse updated successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Warehouse not found" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(id, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, message = "Warehouse deleted successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Warehouse not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id}/set-default")]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _service.SetDefaultAsync(id, TenantId, cancellationToken);
            return Ok(new { success = true, message = "Default warehouse updated successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Warehouse not found" });
        }
    }
}
