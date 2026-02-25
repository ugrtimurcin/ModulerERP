using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.HR.Application.Features.EarningDeductionTypes;

namespace ModulerERP.Api.Controllers.HR;

[ApiController]
[Route("api/hr/earning-deduction-types")]
public class EarningDeductionTypesController : BaseApiController
{
    private readonly ISender _sender;

    public EarningDeductionTypesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var items = await _sender.Send(new GetEarningDeductionTypesQuery(), ct);
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEarningDeductionTypeCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEarningDeductionTypeCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        await _sender.Send(command, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteEarningDeductionTypeCommand(id), ct);
        return NoContent();
    }
}
