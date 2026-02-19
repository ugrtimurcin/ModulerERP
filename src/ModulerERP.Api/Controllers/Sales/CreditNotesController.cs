using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.Sales.Application.Features.CreditNotes.Commands;

namespace ModulerERP.Api.Controllers.Sales;

[Authorize]
[Route("api/sales/credit-notes")]
public class CreditNotesController : BaseApiController
{
    private readonly ISender _sender;
    public CreditNotesController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCreditNoteCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteCreditNoteCommand(id), ct);
        return Ok(new { success = true, message = "Credit note deleted" });
    }

    [HttpPost("{id:guid}/issue")]
    public async Task<IActionResult> Issue(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new IssueCreditNoteCommand(id), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new CancelCreditNoteCommand(id), ct);
        return Ok(new { success = true, data = result });
    }
}
