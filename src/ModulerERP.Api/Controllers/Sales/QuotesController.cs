using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.Sales.Application.Features.Quotes.Commands;
using ModulerERP.Sales.Infrastructure.Features.Quotes.Queries;

namespace ModulerERP.Api.Controllers.Sales;

[Authorize]
[Route("api/sales/quotes")]
public class QuotesController : BaseApiController
{
    private readonly ISender _sender;
    public QuotesController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetAllQuotesQuery(page, pageSize), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetQuoteByIdQuery(id), ct);
        if (result is null) return NotFound(new { success = false, error = "Quote not found" });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuoteCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuoteCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command with { Id = id }, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteQuoteCommand(id), ct);
        return Ok(new { success = true, message = "Quote deleted" });
    }

    [HttpPost("{id:guid}/send")]
    public async Task<IActionResult> Send(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new SendQuoteCommand(id), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new AcceptQuoteCommand(id), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new RejectQuoteCommand(id), ct);
        return Ok(new { success = true, data = result });
    }
}
