using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;

namespace ModulerERP.Api.Controllers.HR;

[Authorize]
[ApiController]
[Route("api/hr/attendance")]
public class AttendanceController : BaseApiController
{
    private readonly IAttendanceService _attendanceService;
    private readonly IAttendanceLogService _logService;

    public AttendanceController(
        IAttendanceService attendanceService,
        IAttendanceLogService logService)
    {
        _attendanceService = attendanceService;
        _logService = logService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAttendance([FromQuery] string? date, CancellationToken ct)
    {
        var targetDate = string.IsNullOrEmpty(date) 
            ? DateTime.UtcNow.Date 
            : DateTime.Parse(date);
        
        var attendance = await _attendanceService.GetByDateAsync(targetDate, ct);
        return Ok(attendance);
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _attendanceService.CheckInAsync(dto.EmployeeId, dto.Time, ct);
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
            await _attendanceService.CheckOutAsync(dto.EmployeeId, dto.Time, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ========== NEW ENDPOINTS (Preparing for Phase 2/3) ==========

    [HttpPost("scan")]
    public async Task<ActionResult<Guid>> LogScan(CreateAttendanceLogDto dto, CancellationToken ct)
    {
        // This was previously in AttendanceLogsController
        var id = await _logService.LogScanAsync(dto, ct);
        return Ok(id);
    }
    
    [HttpGet("logs")]
    public async Task<ActionResult<IReadOnlyList<AttendanceLogDto>>> GetLogs([FromQuery] Guid? employeeId, [FromQuery] DateTime? date, CancellationToken ct)
    {
        // This was previously in AttendanceLogsController
        var result = await _logService.GetLogsAsync(employeeId, date, ct);
        return Ok(result);
    }
}
