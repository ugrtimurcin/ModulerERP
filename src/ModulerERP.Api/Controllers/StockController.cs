using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

//[Authorize]
[Route("api/stock")]
public class StockController : BaseApiController
{
    private readonly IStockService _service;

    public StockController(IStockService service)
    {
        _service = service;
    }

    [HttpPost("movements")]
    public async Task<ActionResult<StockMovementDto>> CreateMovement(CreateStockMovementDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ProcessMovementAsync(dto, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("transfers")]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> CreateTransfer(CreateStockTransferDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ProcessTransferAsync(dto, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("levels")]
    public async Task<ActionResult<IEnumerable<StockLevelDto>>> GetStockLevels(
        [FromQuery] Guid? warehouseId, 
        [FromQuery] Guid? productId, 
        CancellationToken cancellationToken)
    {
        var result = await _service.GetStockLevelsAsync(TenantId, warehouseId, productId, cancellationToken);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("movements")]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetMovements(
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetMovementsAsync(TenantId, warehouseId, productId, fromDate, toDate, cancellationToken);
        return Ok(new { success = true, data = result });
    }
}
