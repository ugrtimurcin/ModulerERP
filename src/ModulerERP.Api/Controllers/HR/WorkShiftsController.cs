using Microsoft.AspNetCore.Mvc;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;

namespace ModulerERP.Api.Controllers.HR;

[ApiController]
[Route("api/hr/work-shifts")]
public class WorkShiftsController : BaseApiController
{
    private readonly IWorkShiftService _workShiftService;

    public WorkShiftsController(IWorkShiftService workShiftService)
    {
        _workShiftService = workShiftService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkShifts(CancellationToken ct)
    {
        var shifts = await _workShiftService.GetAllAsync(ct);
        return Ok(shifts);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetWorkShift(Guid id, CancellationToken ct)
    {
        var shift = await _workShiftService.GetByIdAsync(id, ct);
        if (shift == null) return NotFound();
        return Ok(shift);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkShift([FromBody] CreateWorkShiftDto dto, CancellationToken ct)
    {
        var shift = await _workShiftService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetWorkShift), new { id = shift.Id }, shift);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateWorkShift(Guid id, [FromBody] UpdateWorkShiftDto dto, CancellationToken ct)
    {
        try
        {
            await _workShiftService.UpdateAsync(id, dto, ct);
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
        await _workShiftService.DeleteAsync(id, ct);
        return NoContent();
    }
}
