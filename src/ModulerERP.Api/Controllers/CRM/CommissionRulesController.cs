using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.CRM.Application.Features.CommissionRules.Commands;

namespace ModulerERP.Api.Controllers.CRM;

[Authorize]
[Route("api/crm/commission-rules")]
public class CommissionRulesController : BaseApiController
{
    private readonly ISender _sender;
    public CommissionRulesController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCommissionRuleCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteCommissionRuleCommand(id), ct);
        return Ok(new { success = true, message = "Commission rule deleted" });
    }
}
