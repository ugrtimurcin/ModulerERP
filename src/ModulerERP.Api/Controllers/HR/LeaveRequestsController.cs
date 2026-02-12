using Microsoft.AspNetCore.Mvc;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;

namespace ModulerERP.Api.Controllers.HR;

[ApiController]
[Route("api/hr/leave-requests")]
public class LeaveRequestsController : BaseApiController
{
    private readonly ILeaveRequestService _leaveRequestService;

    public LeaveRequestsController(ILeaveRequestService leaveRequestService)
    {
        _leaveRequestService = leaveRequestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLeaveRequests(CancellationToken ct)
    {
        var requests = await _leaveRequestService.GetAllAsync(ct);
        return Ok(requests);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLeaveRequest(Guid id, CancellationToken ct)
    {
        var request = await _leaveRequestService.GetByIdAsync(id, ct);
        if (request == null) return NotFound();
        return Ok(request);
    }

    [HttpPost]
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

    [HttpPut("{id:guid}/approve")]
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

    [HttpPut("{id:guid}/reject")]
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
}
