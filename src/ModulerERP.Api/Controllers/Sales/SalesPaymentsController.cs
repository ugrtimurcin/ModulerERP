using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.Sales.Application.Features.SalesPayments.Commands;

namespace ModulerERP.Api.Controllers.Sales;

[Authorize]
[Route("api/sales/payments")]
public class SalesPaymentsController : BaseApiController
{
    private readonly ISender _sender;
    public SalesPaymentsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesPaymentCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteSalesPaymentCommand(id), ct);
        return Ok(new { success = true, message = "Payment deleted" });
    }
}
