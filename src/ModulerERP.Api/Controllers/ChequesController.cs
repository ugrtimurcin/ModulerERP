using Microsoft.AspNetCore.Mvc;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using MediatR;


namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/finance/cheques")]
// [Authorize(Roles = "Finance")] // Uncomment when ready
public class ChequesController : BaseApiController
{
    private readonly ISender _sender;

    public ChequesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<List<ChequeDto>>> GetAll(CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.Cheques.Queries.GetChequesQuery(), cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChequeDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.Cheques.Queries.GetChequeByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ChequeDto>> Create([FromBody] CreateChequeDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.Cheques.Commands.CreateChequeCommand(dto, CurrentUserId), cancellationToken));
    }
    
    [HttpPost("status")]
    public async Task<ActionResult<ChequeDto>> UpdateStatus([FromBody] UpdateChequeStatusDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.Cheques.Commands.UpdateChequeStatusCommand(dto, CurrentUserId), cancellationToken));
    }

    [HttpGet("{id}/history")]
    public async Task<ActionResult<List<ModulerERP.Finance.Domain.Entities.ChequeHistory>>> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.Cheques.Queries.GetChequeHistoryQuery(id), cancellationToken));
    }
}
