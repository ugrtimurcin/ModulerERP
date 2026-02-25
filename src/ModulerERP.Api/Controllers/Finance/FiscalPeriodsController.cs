using Microsoft.AspNetCore.Mvc;
using ModulerERP.Finance.Application.DTOs;
using MediatR;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Enums;


namespace ModulerERP.Api.Controllers.Finance;

[Route("api/finance/fiscal-periods")]
public class FiscalPeriodsController : BaseApiController
{
    private readonly ISender _sender;
    private readonly IFiscalPeriodClosingService _closingService;

    public FiscalPeriodsController(ISender sender, IFiscalPeriodClosingService closingService)
    {
        _sender = sender;
        _closingService = closingService;
    }

    [HttpGet]
    public async Task<ActionResult<List<FiscalPeriodDto>>> GetAll(CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.FiscalPeriods.Queries.GetFiscalPeriodsQuery(), cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FiscalPeriodDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.FiscalPeriods.Queries.GetFiscalPeriodByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<FiscalPeriodDto>> Create(CreateFiscalPeriodDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.FiscalPeriods.Commands.CreateFiscalPeriodCommand(dto, CurrentUserId), cancellationToken));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FiscalPeriodDto>> Update(Guid id, UpdateFiscalPeriodDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.FiscalPeriods.Commands.UpdateFiscalPeriodCommand(id, dto), cancellationToken));
    }

    [HttpPost("generate/{year}")]
    public async Task<ActionResult> Generate(int year, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ModulerERP.Finance.Application.Features.FiscalPeriods.Commands.GeneratePeriodsCommand(year, CurrentUserId), cancellationToken);
        if (result.IsSuccess) return Ok();
        return BadRequest(result.Error);
    }

    [HttpPost("{id}/close")]
    public async Task<ActionResult> ClosePeriod(Guid id, CancellationToken cancellationToken)
    {
        var result = await _closingService.ClosePeriodAsync(TenantId, id, cancellationToken);
        if (result.IsSuccess) return Ok();
        return BadRequest(result.Error);
    }
}
