using Microsoft.AspNetCore.Mvc;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.LeaveRequests.Queries;
using ModulerERP.HR.Application.Features.LeaveRequests.Commands;
using ModulerERP.HR.Application.Interfaces;
using MediatR;

namespace ModulerERP.Api.Controllers.HR;

[ApiController]
[Route("api/hr/leave-requests")]
public class LeaveRequestsController : BaseApiController
{
    private readonly ISender _sender;

    public LeaveRequestsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetLeaveRequests([FromQuery] Guid? employeeId, CancellationToken ct)
    {
        var query = new GetLeaveRequestsQuery(employeeId);
        var requests = await _sender.Send(query, ct);
        return Ok(requests);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLeaveRequest(Guid id, CancellationToken ct)
    {
        var query = new GetLeaveRequestByIdQuery(id);
        var request = await _sender.Send(query, ct);
        if (request == null) return NotFound();
        return Ok(request);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveRequestDto dto, CancellationToken ct)
    {
        try
        {
            var command = new CreateLeaveRequestCommand(
                dto.EmployeeId,
                dto.LeavePolicyId,
                dto.StartDate,
                dto.EndDate,
                dto.DaysCount,
                dto.Reason
            );
            var request = await _sender.Send(command, ct);
            return CreatedAtAction(nameof(GetLeaveRequest), new { id = request.Id }, request);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}/approve")]
    public async Task<IActionResult> ApproveLeaveRequest(Guid id, CancellationToken ct)
    {
        try
        {
            var command = new ApproveLeaveRequestCommand(id);
            await _sender.Send(command, ct);
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

    [HttpPut("{id:guid}/reject")]
    public async Task<IActionResult> RejectLeaveRequest(Guid id, CancellationToken ct)
    {
        try
        {
            var command = new RejectLeaveRequestCommand(id);
            await _sender.Send(command, ct);
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
}
