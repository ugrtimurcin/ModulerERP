using Microsoft.AspNetCore.Mvc;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/hr/commission-rules")]
public class CommissionRulesController : ControllerBase
{
    private readonly ICommissionRuleService _service;

    public CommissionRulesController(ICommissionRuleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CommissionRuleDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateCommissionRuleDto dto, CancellationToken cancellationToken)
    {
        var id = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id }, id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<decimal>> Calculate([FromQuery] Guid employeeId, [FromQuery] decimal salesAmount, CancellationToken cancellationToken)
    {
        var result = await _service.CalculateCommissionAsync(employeeId, salesAmount, cancellationToken);
        return Ok(result);
    }
}
