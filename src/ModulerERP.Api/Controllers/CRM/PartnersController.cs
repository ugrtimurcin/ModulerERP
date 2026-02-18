using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.CRM.Application.Features.Partners.Commands;
using ModulerERP.CRM.Infrastructure.Features.Partners.Queries;

namespace ModulerERP.Api.Controllers.CRM;

[Authorize]
[Route("api/crm/partners")]
public class PartnersController : BaseApiController
{
    private readonly ISender _sender;
    public PartnersController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] bool? isCustomer = null, [FromQuery] bool? isSupplier = null,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetPartnersQuery(page, pageSize, isCustomer, isSupplier), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetPartnerByIdQuery(id), ct);
        if (result == null) return NotFound(new { success = false, error = "Partner not found" });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartnerCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePartnerCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command with { Id = id }, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeletePartnerCommand(id), ct);
        return Ok(new { success = true, message = "Partner deleted" });
    }
}
