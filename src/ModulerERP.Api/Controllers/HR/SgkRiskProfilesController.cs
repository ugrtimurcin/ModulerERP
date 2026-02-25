using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.HR.Application.Features.SgkRiskProfiles;

namespace ModulerERP.Api.Controllers.HR;

[ApiController]
[Route("api/hr/sgk-risk-profiles")]
public class SgkRiskProfilesController : BaseApiController
{
    private readonly ISender _sender;

    public SgkRiskProfilesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var items = await _sender.Send(new GetSgkRiskProfilesQuery(), ct);
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSgkRiskProfileCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSgkRiskProfileCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        await _sender.Send(command, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteSgkRiskProfileCommand(id), ct);
        return NoContent();
    }
}
