using Microsoft.AspNetCore.Mvc;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using MediatR;

namespace ModulerERP.Api.Controllers.Finance;

[Route("api/finance/journal-entries")]
public class JournalEntriesController : BaseApiController
{
    private readonly ISender _sender;

    public JournalEntriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<List<JournalEntryDto>>> GetAll(CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.JournalEntries.Queries.GetJournalEntriesQuery(), cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JournalEntryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.JournalEntries.Queries.GetJournalEntryByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<JournalEntryDto>> Create(CreateJournalEntryDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.JournalEntries.Commands.CreateJournalEntryCommand(dto, CurrentUserId), cancellationToken));
    }

}
