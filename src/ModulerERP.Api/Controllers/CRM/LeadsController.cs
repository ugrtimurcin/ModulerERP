using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.CRM.Application.Features.Leads.Commands;
using ModulerERP.CRM.Infrastructure.Features.Leads.Queries;

namespace ModulerERP.Api.Controllers.CRM;

[Authorize]
[Route("api/crm/leads")]
public class LeadsController : BaseApiController
{
    private readonly ISender _sender;
    public LeadsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null, [FromQuery] Guid? assignedUserId = null,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetLeadsQuery(page, pageSize, status, assignedUserId), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var lead = await _sender.Send(new GetLeadByIdQuery(id), ct);
        if (lead == null) return NotFound(new { success = false, error = "Lead not found" });
        return Ok(new { success = true, data = lead });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeadCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeadCommand command, CancellationToken ct)
    {
        var cmd = command with { Id = id };
        var result = await _sender.Send(cmd, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteLeadCommand(id), ct);
        return Ok(new { success = true, message = "Lead deleted" });
    }

    [HttpPost("{id:guid}/convert")]
    public async Task<IActionResult> Convert(Guid id, CancellationToken ct)
    {
        var partnerId = await _sender.Send(new ConvertLeadToPartnerCommand(id), ct);
        return Ok(new { success = true, data = new { partnerId } });
    }
}
