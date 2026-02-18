using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.CRM.Application.Features.Contacts.Commands;
using ModulerERP.CRM.Infrastructure.Features.Contacts.Queries;

namespace ModulerERP.Api.Controllers.CRM;

[Authorize]
[Route("api/crm/contacts")]
public class ContactsController : BaseApiController
{
    private readonly ISender _sender;
    public ContactsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] Guid? partnerId = null, CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetContactsQuery(page, pageSize, partnerId), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetContactByIdQuery(id), ct);
        if (result == null) return NotFound(new { success = false, error = "Contact not found" });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContactCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command with { Id = id }, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteContactCommand(id), ct);
        return Ok(new { success = true, message = "Contact deleted" });
    }
}
