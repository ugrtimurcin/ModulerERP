using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Payroll.Commands;
using ModulerERP.HR.Application.Features.Payroll.Queries;

namespace ModulerERP.Api.Controllers.HR;

[Authorize]
[ApiController]
[Route("api/hr/payroll")]
public class PayrollController : BaseApiController
{
    private readonly ISender _sender;

    public PayrollController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetPayrollRuns([FromQuery] int? year, CancellationToken ct)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var query = new GetPayrollsQuery(targetYear);
        var payrolls = await _sender.Send(query, ct);
        return Ok(payrolls);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPayrollRun(Guid id, CancellationToken ct)
    {
        var query = new GetPayrollByIdQuery(id);
        var payroll = await _sender.Send(query, ct);
        if (payroll == null) return NotFound();
        return Ok(payroll);
    }

    [HttpGet("{id:guid}/slips")]
    public async Task<IActionResult> GetPayrollSlips(Guid id, CancellationToken ct)
    {
        var query = new GetPayrollEntriesQuery(id);
        var slips = await _sender.Send(query, ct);
        return Ok(slips);
    }

    [HttpPost("run")]
    public async Task<IActionResult> RunPayroll([FromBody] RunPayrollDto dto, CancellationToken ct)
    {
        try
        {
            var command = new RunPayrollCommand(dto);
            var payroll = await _sender.Send(command, ct);
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
        var query = new GetPayrollSummaryQuery(year, month);
        var summary = await _sender.Send(query, ct);
        return Ok(summary);
    }
}
