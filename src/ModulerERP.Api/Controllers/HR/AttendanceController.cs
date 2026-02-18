using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Attendance.Queries;
using ModulerERP.HR.Application.Features.Attendance.Commands;

namespace ModulerERP.Api.Controllers.HR;

[Authorize]
[ApiController]
[Route("api/hr/attendance")]
public class AttendanceController : BaseApiController
{
    private readonly ISender _sender;

    public AttendanceController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAttendance([FromQuery] string? date, CancellationToken ct)
    {
        var targetDate = string.IsNullOrEmpty(date) 
            ? DateTime.UtcNow.Date 
            : DateTime.Parse(date);
        
        var query = new GetAttendanceByDateQuery(targetDate);
        var attendance = await _sender.Send(query, ct);
        return Ok(attendance);
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto, CancellationToken ct)
    {
        try
        {
            var command = new CheckInCommand(dto.EmployeeId, dto.Time);
            var result = await _sender.Send(command, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutDto dto, CancellationToken ct)
    {
        try
        {
            var command = new CheckOutCommand(dto.EmployeeId, dto.Time);
            await _sender.Send(command, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("scan")]
    public async Task<ActionResult<Guid>> LogScan(CreateAttendanceLogDto dto, CancellationToken ct)
    {
        var command = new LogAttendanceScanCommand(dto);
        var id = await _sender.Send(command, ct);
        return Ok(id);
    }
    
    [HttpGet("logs")]
    public async Task<ActionResult<IReadOnlyList<AttendanceLogDto>>> GetLogs([FromQuery] Guid? employeeId, [FromQuery] DateTime? date, CancellationToken ct)
    {
        var query = new GetAttendanceLogsQuery(employeeId, date);
        var result = await _sender.Send(query, ct);
        return Ok(result);
    }
}
