using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/hr")]
//[Authorize]
public class HRController : BaseApiController
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly IAttendanceService _attendanceService;
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly IPayrollService _payrollService;
    private readonly IWorkShiftService _workShiftService;
    private readonly IAdvanceRequestService _advanceRequestService;

    public HRController(
        IEmployeeService employeeService, 
        IDepartmentService departmentService,
        IAttendanceService attendanceService,
        ILeaveRequestService leaveRequestService,
        IPayrollService payrollService,
        IWorkShiftService workShiftService,
        IAdvanceRequestService advanceRequestService)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
        _attendanceService = attendanceService;
        _leaveRequestService = leaveRequestService;
        _payrollService = payrollService;
        _workShiftService = workShiftService;
        _advanceRequestService = advanceRequestService;
    }

    // ==================== EMPLOYEES ====================

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees(CancellationToken ct)
    {
        var employees = await _employeeService.GetAllAsync(ct);
        return Ok(employees);
    }

    [HttpGet("employees/{id:guid}")]
    public async Task<IActionResult> GetEmployee(Guid id, CancellationToken ct)
    {
        var employee = await _employeeService.GetByIdAsync(id, ct);
        if (employee == null) return NotFound();
        return Ok(employee);
    }

    [HttpPost("employees")]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto dto, CancellationToken ct)
    {
        var employee = await _employeeService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    [HttpPut("employees/{id:guid}")]
    public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeDto dto, CancellationToken ct)
    {
        await _employeeService.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    [HttpPut("employees/{id:guid}/generate-qr")]
    public async Task<IActionResult> GenerateQrToken(Guid id, CancellationToken ct)
    {
        var token = await _employeeService.GenerateQrTokenAsync(id, ct);
        return Ok(new { token });
    }

    [HttpGet("employees/lookup")]
    public async Task<IActionResult> GetEmployeesLookup(CancellationToken ct)
    {
        // TODO: Move projection to Service layer in future. For now, fetch all and project.
        var employees = await _employeeService.GetAllAsync(ct);
        
        Console.WriteLine($"[HRController] GetEmployeesLookup - Found {employees.Count()} employees.");
        
        var result = employees.Select(e => new 
        { 
            e.Id, 
            e.FirstName, 
            e.LastName, 
            Position = e.JobTitle 
        });
        return Ok(new { success = true, data = result });
    }

    // ==================== DEPARTMENTS ====================

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments(CancellationToken ct)
    {
        var departments = await _departmentService.GetAllAsync(ct);
        return Ok(departments);
    }

    [HttpGet("departments/{id:guid}")]
    public async Task<IActionResult> GetDepartment(Guid id, CancellationToken ct)
    {
        var department = await _departmentService.GetByIdAsync(id, ct);
        if (department == null) return NotFound();
        return Ok(department);
    }

    [HttpPost("departments")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto dto, CancellationToken ct)
    {
        var department = await _departmentService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, department);
    }

    [HttpPut("departments/{id:guid}")]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentDto dto, CancellationToken ct)
    {
        await _departmentService.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    [HttpDelete("departments/{id:guid}")]
    public async Task<IActionResult> DeleteDepartment(Guid id, CancellationToken ct)
    {
        await _departmentService.DeleteAsync(id, ct);
        return NoContent();
    }

    // ==================== ATTENDANCE ====================

    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendance([FromQuery] string? date, CancellationToken ct)
    {
        var targetDate = string.IsNullOrEmpty(date) 
            ? DateTime.UtcNow.Date 
            : DateTime.Parse(date);
        
        var attendance = await _attendanceService.GetByDateAsync(targetDate, ct);
        return Ok(attendance);
    }

    [HttpPost("attendance/check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _attendanceService.CheckInAsync(dto.EmployeeId, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("attendance/check-out")]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutDto dto, CancellationToken ct)
    {
        try
        {
            await _attendanceService.CheckOutAsync(dto.EmployeeId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ==================== LEAVE REQUESTS ====================

    [HttpGet("leave-requests")]
    public async Task<IActionResult> GetLeaveRequests(CancellationToken ct)
    {
        var requests = await _leaveRequestService.GetAllAsync(ct);
        return Ok(requests);
    }

    [HttpGet("leave-requests/{id:guid}")]
    public async Task<IActionResult> GetLeaveRequest(Guid id, CancellationToken ct)
    {
        var request = await _leaveRequestService.GetByIdAsync(id, ct);
        if (request == null) return NotFound();
        return Ok(request);
    }

    [HttpPost("leave-requests")]
    public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveRequestDto dto, CancellationToken ct)
    {
        try
        {
            var request = await _leaveRequestService.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetLeaveRequest), new { id = request.Id }, request);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("leave-requests/{id:guid}/approve")]
    public async Task<IActionResult> ApproveLeaveRequest(Guid id, CancellationToken ct)
    {
        try
        {
            await _leaveRequestService.ApproveAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("leave-requests/{id:guid}/reject")]
    public async Task<IActionResult> RejectLeaveRequest(Guid id, CancellationToken ct)
    {
        try
        {
            await _leaveRequestService.RejectAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ==================== PAYROLL ====================

    [HttpGet("payroll")]
    public async Task<IActionResult> GetPayrollRuns([FromQuery] int? year, CancellationToken ct)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var payrolls = await _payrollService.GetByYearAsync(targetYear, ct);
        return Ok(payrolls);
    }

    [HttpGet("payroll/{id:guid}")]
    public async Task<IActionResult> GetPayrollRun(Guid id, CancellationToken ct)
    {
        var payroll = await _payrollService.GetByIdAsync(id, ct);
        if (payroll == null) return NotFound();
        return Ok(payroll);
    }

    [HttpGet("payroll/{id:guid}/slips")]
    public async Task<IActionResult> GetPayrollSlips(Guid id, CancellationToken ct)
    {
        var slips = await _payrollService.GetEntriesAsync(id, ct);
        return Ok(slips);
    }

    [HttpPost("payroll/run")]
    public async Task<IActionResult> RunPayroll([FromBody] RunPayrollDto dto, CancellationToken ct)
    {
        try
        {
            var payroll = await _payrollService.RunPayrollAsync(dto, ct);
            return CreatedAtAction(nameof(GetPayrollRun), new { id = payroll.Id }, payroll);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("payroll/summary")]
    public async Task<IActionResult> GetPayrollSummary([FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var summary = await _payrollService.GetSummaryAsync(year, month, ct);
        return Ok(summary);
    }

    // ==================== WORK SHIFTS ====================

    [HttpGet("work-shifts")]
    public async Task<IActionResult> GetWorkShifts(CancellationToken ct)
    {
        var shifts = await _workShiftService.GetAllAsync(ct);
        return Ok(shifts);
    }

    [HttpGet("work-shifts/{id:guid}")]
    public async Task<IActionResult> GetWorkShift(Guid id, CancellationToken ct)
    {
        var shift = await _workShiftService.GetByIdAsync(id, ct);
        if (shift == null) return NotFound();
        return Ok(shift);
    }

    [HttpPost("work-shifts")]
    public async Task<IActionResult> CreateWorkShift([FromBody] CreateWorkShiftDto dto, CancellationToken ct)
    {
        var shift = await _workShiftService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetWorkShift), new { id = shift.Id }, shift);
    }

    [HttpPut("work-shifts/{id:guid}")]
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

    [HttpDelete("work-shifts/{id:guid}")]
    public async Task<IActionResult> DeleteWorkShift(Guid id, CancellationToken ct)
    {
        await _workShiftService.DeleteAsync(id, ct);
        return NoContent();
    }

    // ==================== ADVANCE REQUESTS ====================

    [HttpGet("employees/{employeeId:guid}/advance-requests")]
    public async Task<IActionResult> GetAdvanceRequestsByEmployee(Guid employeeId, CancellationToken ct)
    {
        var requests = await _advanceRequestService.GetByEmployeeAsync(employeeId, ct);
        return Ok(requests);
    }

    [HttpGet("advance-requests")]
    public async Task<IActionResult> GetAdvanceRequests(CancellationToken ct)
    {
        var requests = await _advanceRequestService.GetAllAsync(ct);
        return Ok(requests);
    }

    [HttpPost("advance-requests")]
    public async Task<IActionResult> CreateAdvanceRequest([FromBody] CreateAdvanceRequestDto dto, CancellationToken ct)
    {
        var request = await _advanceRequestService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetAdvanceRequests), null, request);
    }

    [HttpPut("advance-requests/{id:guid}/approve")]
    public async Task<IActionResult> ApproveAdvanceRequest(Guid id, CancellationToken ct)
    {
        await _advanceRequestService.ApproveAsync(id, ct);
        return NoContent();
    }

    [HttpPut("advance-requests/{id:guid}/reject")]
    public async Task<IActionResult> RejectAdvanceRequest(Guid id, CancellationToken ct)
    {
        await _advanceRequestService.RejectAsync(id, ct);
        return NoContent();
    }

    [HttpPut("advance-requests/{id:guid}/paid")]
    public async Task<IActionResult> MarkAdvanceRequestPaid(Guid id, CancellationToken ct)
    {
        await _advanceRequestService.MarkAsPaidAsync(id, ct);
        return NoContent();
    }
}
