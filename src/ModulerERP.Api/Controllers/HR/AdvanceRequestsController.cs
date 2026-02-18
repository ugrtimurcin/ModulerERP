using Microsoft.AspNetCore.Mvc;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.AdvanceRequests.Commands;
using ModulerERP.HR.Application.Features.AdvanceRequests.Queries;
using MediatR;

namespace ModulerERP.Api.Controllers.HR;

[ApiController]
[Route("api/hr/advance-requests")]
public class AdvanceRequestsController : BaseApiController
{
    private readonly ISender _sender;

    public AdvanceRequestsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("by-employee/{employeeId:guid}")]
    public async Task<IActionResult> GetAdvanceRequestsByEmployee(Guid employeeId, CancellationToken ct)
    {
        var query = new GetAdvanceRequestsQuery(employeeId);
        var requests = await _sender.Send(query, ct);
        return Ok(requests);
    }

    [HttpGet]
    public async Task<IActionResult> GetAdvanceRequests(CancellationToken ct)
    {
        var query = new GetAdvanceRequestsQuery(null);
        var requests = await _sender.Send(query, ct);
        return Ok(requests);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdvanceRequest([FromBody] CreateAdvanceRequestDto dto, CancellationToken ct)
    {
        var command = new CreateAdvanceRequestCommand(dto.EmployeeId, dto.Amount, dto.Description);
        var request = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetAdvanceRequests), null, request);
    }

    [HttpPut("{id:guid}/approve")]
    public async Task<IActionResult> ApproveAdvanceRequest(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ApproveAdvanceRequestCommand(id), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/reject")]
    public async Task<IActionResult> RejectAdvanceRequest(Guid id, CancellationToken ct)
    {
        await _sender.Send(new RejectAdvanceRequestCommand(id), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/paid")]
    public async Task<IActionResult> MarkAdvanceRequestPaid(Guid id, CancellationToken ct)
    {
        await _sender.Send(new PayAdvanceRequestCommand(id), ct);
        return NoContent();
    }
}
