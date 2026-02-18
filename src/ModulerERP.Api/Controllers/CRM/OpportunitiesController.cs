using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.CRM.Application.Features.Opportunities.Commands;
using ModulerERP.CRM.Infrastructure.Features.Opportunities.Queries;

namespace ModulerERP.Api.Controllers.CRM;

[Authorize]
[Route("api/crm/opportunities")]
public class OpportunitiesController : BaseApiController
{
    private readonly ISender _sender;
    public OpportunitiesController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? stage = null, [FromQuery] Guid? assignedUserId = null,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetOpportunitiesQuery(page, pageSize, stage, assignedUserId), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetOpportunityByIdQuery(id), ct);
        if (result == null) return NotFound(new { success = false, error = "Opportunity not found" });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOpportunityCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command with { Id = id }, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteOpportunityCommand(id), ct);
        return Ok(new { success = true, message = "Opportunity deleted" });
    }

    [HttpPatch("{id:guid}/stage")]
    public async Task<IActionResult> UpdateStage(Guid id, [FromBody] string stage, CancellationToken ct)
    {
        await _sender.Send(new UpdateOpportunityStageCommand(id, stage), ct);
        return Ok(new { success = true });
    }
}
