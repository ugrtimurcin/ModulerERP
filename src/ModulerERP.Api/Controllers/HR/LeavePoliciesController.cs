using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.HR.Application.Features.LeavePolicies;

namespace ModulerERP.Api.Controllers.HR;

[ApiController]
[Route("api/hr/leave-policies")]
public class LeavePoliciesController : BaseApiController
{
    private readonly ISender _sender;

    public LeavePoliciesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var policies = await _sender.Send(new GetLeavePoliciesQuery(), ct);
        return Ok(policies);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeavePolicyCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeavePolicyCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        await _sender.Send(command, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteLeavePolicyCommand(id), ct);
        return NoContent();
    }
}
