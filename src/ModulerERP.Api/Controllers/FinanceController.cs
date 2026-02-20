using Microsoft.AspNetCore.Mvc;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using MediatR;

namespace ModulerERP.Api.Controllers;

public class FinanceController : BaseApiController
{
    private readonly ISender _sender;

    public FinanceController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("journal-entries")]
    public async Task<ActionResult<List<JournalEntryDto>>> GetAll(CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.JournalEntries.Queries.GetJournalEntriesQuery(), cancellationToken));
    }

    [HttpGet("journal-entries/{id}")]
    public async Task<ActionResult<JournalEntryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.JournalEntries.Queries.GetJournalEntryByIdQuery(id), cancellationToken));
    }

    [HttpPost("journal-entries")]
    public async Task<ActionResult<JournalEntryDto>> Create(CreateJournalEntryDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.JournalEntries.Commands.CreateJournalEntryCommand(dto, CurrentUserId), cancellationToken));
    }

}
