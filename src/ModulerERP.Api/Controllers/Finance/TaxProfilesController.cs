using Microsoft.AspNetCore.Mvc;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Features.TaxProfiles.Commands;
using ModulerERP.Finance.Application.Features.TaxProfiles.Queries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/tax-profiles")]
public class TaxProfilesController : BaseApiController
{
    private readonly ISender _sender;

    public TaxProfilesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<List<TaxProfileDto>>> GetAll(CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new GetTaxProfilesQuery(), cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaxProfileDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new GetTaxProfileByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<TaxProfileDto>> Create([FromBody] CreateTaxProfileDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new CreateTaxProfileCommand(dto, CurrentUserId), cancellationToken));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaxProfileDto>> Update(Guid id, [FromBody] UpdateTaxProfileDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new UpdateTaxProfileCommand(id, dto, CurrentUserId), cancellationToken));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Guid>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new DeleteTaxProfileCommand(id), cancellationToken));
    }
}
