using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.CRM.Application.Features.SupportTickets.Commands;
using ModulerERP.CRM.Application.Features.TicketMessages.Commands;
using ModulerERP.CRM.Infrastructure.Features.SupportTickets.Queries;
using ModulerERP.CRM.Infrastructure.Features.TicketMessages.Queries;

namespace ModulerERP.Api.Controllers.CRM;

[Authorize]
[Route("api/crm/tickets")]
public class SupportTicketsController : BaseApiController
{
    private readonly ISender _sender;
    public SupportTicketsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] Guid? partnerId = null, CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetSupportTicketsQuery(page, pageSize, partnerId), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetSupportTicketByIdQuery(id), ct);
        if (result == null) return NotFound(new { success = false, error = "Ticket not found" });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupportTicketCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveSupportTicketCommand command, CancellationToken ct)
    {
        await _sender.Send(command with { Id = id }, ct);
        return Ok(new { success = true, message = "Ticket resolved" });
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct)
    {
        await _sender.Send(new CloseSupportTicketCommand(id), ct);
        return Ok(new { success = true, message = "Ticket closed" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteSupportTicketCommand(id), ct);
        return Ok(new { success = true, message = "Ticket deleted" });
    }

    // ── Ticket Messages ──
    [HttpGet("{ticketId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid ticketId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetTicketMessagesQuery(ticketId), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{ticketId:guid}/messages")]
    public async Task<IActionResult> AddMessage(Guid ticketId, [FromBody] CreateTicketMessageCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command with { TicketId = ticketId }, ct);
        return Ok(new { success = true, data = result });
    }
}
