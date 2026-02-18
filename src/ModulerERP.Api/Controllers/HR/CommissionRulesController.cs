using Microsoft.AspNetCore.Mvc;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.CommissionRules.Commands;
using ModulerERP.HR.Application.Features.CommissionRules.Queries;
using MediatR;
using ModulerERP.HR.Domain.Enums; // CommissionBasis enum

namespace ModulerERP.Api.Controllers.HR;

[ApiController]
[Route("api/hr/commission-rules")]
public class CommissionRulesController : BaseApiController
{
    private readonly ISender _sender;

    public CommissionRulesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetCommissionRules(CancellationToken ct)
    {
        var query = new GetCommissionRulesQuery();
        var rules = await _sender.Send(query, ct);
        return Ok(rules);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCommissionRule([FromBody] CreateCommissionRuleDto dto, CancellationToken ct)
    {
        var command = new CreateCommissionRuleCommand(dto.Role, dto.MinTargetAmount, dto.Percentage, dto.Basis);
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetCommissionRules), new { id }, id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCommissionRule(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteCommissionRuleCommand(id), ct);
        return NoContent();
    }
    
    [HttpGet("calculate")]
    public async Task<IActionResult> CalculateCommission([FromQuery] Guid employeeId, [FromQuery] decimal salesAmount, CancellationToken ct)
    {
        var query = new CalculateCommissionQuery(employeeId, salesAmount);
        var amount = await _sender.Send(query, ct);
        return Ok(amount);
    }
}
