using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;

namespace ModulerERP.Api.Controllers.HR;

[Authorize]
[ApiController]
[Route("api/hr/payroll")]
public class PayrollController : BaseApiController
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPayrollRuns([FromQuery] int? year, CancellationToken ct)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var payrolls = await _payrollService.GetByYearAsync(targetYear, ct);
        return Ok(payrolls);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPayrollRun(Guid id, CancellationToken ct)
    {
        var payroll = await _payrollService.GetByIdAsync(id, ct);
        if (payroll == null) return NotFound();
        return Ok(payroll);
    }

    [HttpGet("{id:guid}/slips")]
    public async Task<IActionResult> GetPayrollSlips(Guid id, CancellationToken ct)
    {
        var slips = await _payrollService.GetEntriesAsync(id, ct);
        return Ok(slips);
    }

    [HttpPost("run")]
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

    [HttpGet("summary")]
    public async Task<IActionResult> GetPayrollSummary([FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var summary = await _payrollService.GetSummaryAsync(year, month, ct);
        return Ok(summary);
    }
}
