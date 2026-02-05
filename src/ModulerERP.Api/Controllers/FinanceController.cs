using Microsoft.AspNetCore.Mvc;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

public class FinanceController : BaseApiController
{
    private readonly IJournalEntryService _journalEntryService;

    public FinanceController(IJournalEntryService journalEntryService)
    {
        _journalEntryService = journalEntryService;
    }

    [HttpGet("journal-entries")]
    public async Task<ActionResult<List<JournalEntryDto>>> GetAll(CancellationToken cancellationToken)
    {
        return HandleResult(await _journalEntryService.GetAllAsync(cancellationToken));
    }

    [HttpGet("journal-entries/{id}")]
    public async Task<ActionResult<JournalEntryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _journalEntryService.GetByIdAsync(id, cancellationToken));
    }
    [HttpPost("journal-entries")]
    public async Task<ActionResult<JournalEntryDto>> Create(CreateJournalEntryDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _journalEntryService.CreateManualEntryAsync(dto, CurrentUserId, cancellationToken));
    }
}
