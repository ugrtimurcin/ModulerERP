using Microsoft.AspNetCore.Mvc;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;

namespace ModulerERP.Api.Controllers.HR;

[ApiController]
[Route("api/hr/advance-requests")]
public class AdvanceRequestsController : BaseApiController
{
    private readonly IAdvanceRequestService _advanceRequestService;

    public AdvanceRequestsController(IAdvanceRequestService advanceRequestService)
    {
        _advanceRequestService = advanceRequestService;
    }

    [HttpGet("by-employee/{employeeId:guid}")]
    public async Task<IActionResult> GetAdvanceRequestsByEmployee(Guid employeeId, CancellationToken ct)
    {
        var requests = await _advanceRequestService.GetByEmployeeAsync(employeeId, ct);
        return Ok(requests);
    }

    [HttpGet]
    public async Task<IActionResult> GetAdvanceRequests(CancellationToken ct)
    {
        var requests = await _advanceRequestService.GetAllAsync(ct);
        return Ok(requests);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdvanceRequest([FromBody] CreateAdvanceRequestDto dto, CancellationToken ct)
    {
        var request = await _advanceRequestService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetAdvanceRequests), null, request);
    }

    [HttpPut("{id:guid}/approve")]
    public async Task<IActionResult> ApproveAdvanceRequest(Guid id, CancellationToken ct)
    {
        await _advanceRequestService.ApproveAsync(id, ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/reject")]
    public async Task<IActionResult> RejectAdvanceRequest(Guid id, CancellationToken ct)
    {
        await _advanceRequestService.RejectAsync(id, ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/paid")]
    public async Task<IActionResult> MarkAdvanceRequestPaid(Guid id, CancellationToken ct)
    {
        await _advanceRequestService.MarkAsPaidAsync(id, ct);
        return NoContent();
    }
}
