using Microsoft.AspNetCore.Mvc;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/hr/attendance-logs")]
public class AttendanceLogsController : ControllerBase
{
    private readonly IAttendanceLogService _service;

    public AttendanceLogsController(IAttendanceLogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AttendanceLogDto>>> GetLogs([FromQuery] Guid? employeeId, [FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        var result = await _service.GetLogsAsync(employeeId, date, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> LogScan(CreateAttendanceLogDto dto, CancellationToken cancellationToken)
    {
        var id = await _service.LogScanAsync(dto, cancellationToken);
        return Ok(id);
    }
}
