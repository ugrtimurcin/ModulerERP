using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.Sales.Application.Features.SalesReturns.Commands;

namespace ModulerERP.Api.Controllers.Sales;

[Authorize]
[Route("api/sales/returns")]
public class SalesReturnsController : BaseApiController
{
    private readonly ISender _sender;
    public SalesReturnsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesReturnCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteSalesReturnCommand(id), ct);
        return Ok(new { success = true, message = "Sales return deleted" });
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new ApproveSalesReturnCommand(id), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id:guid}/receive")]
    public async Task<IActionResult> Receive(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new ReceiveSalesReturnCommand(id), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id:guid}/refund")]
    public async Task<IActionResult> Refund(Guid id, [FromBody] decimal amount, CancellationToken ct)
    {
        var result = await _sender.Send(new RefundSalesReturnCommand(id, amount), ct);
        return Ok(new { success = true, data = result });
    }
}
