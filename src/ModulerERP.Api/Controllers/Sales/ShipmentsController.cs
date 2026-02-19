using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.Sales.Application.Features.Shipments.Commands;
using ModulerERP.Sales.Infrastructure.Features.Shipments.Queries;

namespace ModulerERP.Api.Controllers.Sales;

[Authorize]
[Route("api/sales/shipments")]
public class ShipmentsController : BaseApiController
{
    private readonly ISender _sender;
    public ShipmentsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetAllShipmentsQuery(page, pageSize), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetShipmentByIdQuery(id), ct);
        if (result is null) return NotFound(new { success = false, error = "Shipment not found" });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShipmentCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteShipmentCommand(id), ct);
        return Ok(new { success = true, message = "Shipment deleted" });
    }

    [HttpPost("{id:guid}/ship")]
    public async Task<IActionResult> Ship(Guid id, [FromQuery] string? trackingNumber, CancellationToken ct)
    {
        var result = await _sender.Send(new ShipShipmentCommand(id, trackingNumber), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id:guid}/deliver")]
    public async Task<IActionResult> Deliver(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new DeliverShipmentCommand(id), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id:guid}/waybill")]
    public async Task<IActionResult> SetWaybill(Guid id, [FromBody] SetWaybillCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command with { Id = id }, ct);
        return Ok(new { success = true, data = result });
    }
}
