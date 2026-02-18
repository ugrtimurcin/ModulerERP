using Microsoft.AspNetCore.Mvc;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.TaxRules.Commands;
using ModulerERP.HR.Application.Features.TaxRules.Queries;
using MediatR;

namespace ModulerERP.Api.Controllers.HR;

[ApiController]
[Route("api/hr/tax-rules")]
public class TaxRulesController : BaseApiController
{
    private readonly ISender _sender;

    public TaxRulesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetTaxRules(CancellationToken ct)
    {
        var query = new GetTaxRulesQuery();
        var rules = await _sender.Send(query, ct);
        return Ok(rules);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTaxRule(Guid id, CancellationToken ct)
    {
        var query = new GetTaxRuleByIdQuery(id);
        var rule = await _sender.Send(query, ct);
        if (rule == null) return NotFound();
        return Ok(rule);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTaxRule([FromBody] CreateTaxRuleDto dto, CancellationToken ct)
    {
        var command = new CreateTaxRuleCommand(dto.Name, dto.LowerLimit, dto.UpperLimit, dto.Rate, dto.Order, dto.EffectiveFrom);
        var rule = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetTaxRule), new { id = rule.Id }, rule);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTaxRule(Guid id, [FromBody] UpdateTaxRuleDto dto, CancellationToken ct)
    {
        if (id != dto.Id && dto.Id != Guid.Empty) return BadRequest("ID mismatch");
        
        var command = new UpdateTaxRuleCommand(id, dto.Name, dto.LowerLimit, dto.UpperLimit, dto.Rate, dto.Order, dto.EffectiveFrom, dto.EffectiveTo);
        var rule = await _sender.Send(command, ct);
        return Ok(rule);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTaxRule(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteTaxRuleCommand(id), ct);
        return NoContent();
    }
}
