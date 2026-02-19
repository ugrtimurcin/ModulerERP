using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.Sales.Application.Features.Invoices.Commands;
using ModulerERP.Sales.Infrastructure.Features.Invoices.Queries;

namespace ModulerERP.Api.Controllers.Sales;

[Authorize]
[Route("api/sales/invoices")]
public class InvoicesController : BaseApiController
{
    private readonly ISender _sender;
    public InvoicesController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetAllInvoicesQuery(page, pageSize), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetInvoiceByIdQuery(id), ct);
        if (result is null) return NotFound(new { success = false, error = "Invoice not found" });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteInvoiceCommand(id), ct);
        return Ok(new { success = true, message = "Invoice deleted" });
    }

    [HttpPost("{id:guid}/issue")]
    public async Task<IActionResult> Issue(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new IssueInvoiceCommand(id), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id:guid}/record-payment")]
    public async Task<IActionResult> RecordPayment(Guid id, [FromBody] decimal amount, CancellationToken ct)
    {
        var result = await _sender.Send(new RecordInvoicePaymentCommand(id, amount), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new CancelInvoiceCommand(id), ct);
        return Ok(new { success = true, data = result });
    }
}
