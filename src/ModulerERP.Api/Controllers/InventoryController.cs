using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ModulerERP.SystemCore.Application.Constants;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Features.Inventory.Commands;
using ModulerERP.Inventory.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

[Route("api/inventory")]
[Authorize]
public class InventoryController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IStockService _queryService; // Use service for queries temporarily

    public InventoryController(IMediator mediator, IStockService queryService)
    {
        _mediator = mediator;
        _queryService = queryService;
    }

    [HttpPost("transfers")]
    [Authorize(Policy = Permissions.Inventory.Transfers)]
    public async Task<ActionResult<Guid>> CreateTransfer(CreateStockTransferDto dto)
    {
        var command = new CreateStockTransferCommand(dto);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(new { success = true, data = result.Value }) : BadRequest(new { success = false, error = result.Error });
    }

    [HttpPost("movements")]
    [Authorize(Policy = Permissions.Inventory.StockAdjust)]
    public async Task<ActionResult<Guid>> CreateMovement(CreateStockMovementDto dto)
    {
         // Assuming AdjustStockCommand handles generic movements or I need CreateStockMovementCommand
         // Plan said "AdjustStockCommand".
         // If dto.Type is Adjustment, use AdjustStockCommand.
         // If it's pure movement creation (legacy), maybe use service or create generic command.
         var command = new AdjustStockCommand(dto);
         var result = await _mediator.Send(command);
         return result.IsSuccess ? Ok(new { success = true, data = result.Value }) : BadRequest(new { success = false, error = result.Error });
    }

    [HttpPost("transfers/{id}/receive")]
    [Authorize(Policy = Permissions.Inventory.Transfers)]
    public async Task<ActionResult<Guid>> ReceiveGoods(Guid id)
    {
        var command = new ReceiveGoodsCommand(id);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(new { success = true, data = result.Value }) : BadRequest(new { success = false, error = result.Error });
    }

    [HttpGet("levels")]
    [Authorize(Policy = Permissions.Inventory.View)]
    public async Task<ActionResult<IEnumerable<StockLevelDto>>> GetStockLevels(
        [FromQuery] Guid? warehouseId, 
        [FromQuery] Guid? productId, 
        CancellationToken cancellationToken)
    {
        // Use existing service for query
        var result = await _queryService.GetStockLevelsAsync(TenantId, warehouseId, productId, cancellationToken);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("movements")]
    [Authorize(Policy = Permissions.Inventory.View)]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetMovements(
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetMovementsAsync(TenantId, warehouseId, productId, fromDate, toDate, cancellationToken);
        return Ok(new { success = true, data = result });
    }
}
