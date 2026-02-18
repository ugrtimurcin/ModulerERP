using Microsoft.AspNetCore.Mvc;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.WorkShifts.Commands;
using ModulerERP.HR.Application.Features.WorkShifts.Queries;
using MediatR;

namespace ModulerERP.Api.Controllers.HR;

[ApiController]
[Route("api/hr/work-shifts")]
public class WorkShiftsController : BaseApiController
{
    private readonly ISender _sender;

    public WorkShiftsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkShifts(CancellationToken ct)
    {
        var query = new GetWorkShiftsQuery();
        var shifts = await _sender.Send(query, ct);
        return Ok(shifts);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetWorkShift(Guid id, CancellationToken ct)
    {
        var query = new GetWorkShiftByIdQuery(id);
        var shift = await _sender.Send(query, ct);
        if (shift == null) return NotFound();
        return Ok(shift);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkShift([FromBody] CreateWorkShiftDto dto, CancellationToken ct)
    {
        var command = new CreateWorkShiftCommand(dto.Name, dto.StartTime, dto.EndTime, dto.BreakMinutes);
        var shift = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetWorkShift), new { id = shift.Id }, shift);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateWorkShift(Guid id, [FromBody] UpdateWorkShiftDto dto, CancellationToken ct)
    {
        try
        {
            var command = new UpdateWorkShiftCommand(id, dto.Name, dto.StartTime, dto.EndTime, dto.BreakMinutes);
            await _sender.Send(command, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteWorkShift(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteWorkShiftCommand(id), ct);
        return NoContent();
    }
}
