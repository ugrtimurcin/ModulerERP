using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.CRM.Application.Features.SaleAgents.Commands;

namespace ModulerERP.Api.Controllers.CRM;

[Authorize]
[Route("api/crm/sale-agents")]
public class SaleAgentsController : BaseApiController
{
    private readonly ISender _sender;
    public SaleAgentsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleAgentCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteSaleAgentCommand(id), ct);
        return Ok(new { success = true, message = "Sale agent deleted" });
    }
}
