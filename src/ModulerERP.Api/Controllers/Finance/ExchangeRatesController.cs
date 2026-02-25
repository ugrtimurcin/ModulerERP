using Microsoft.AspNetCore.Mvc;
using ModulerERP.Finance.Application.DTOs;
using MediatR;

namespace ModulerERP.Api.Controllers.Finance;

[Route("api/finance/exchange-rates")]
public class ExchangeRatesController : BaseApiController
{
    private readonly ISender _sender;

    public ExchangeRatesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<List<ExchangeRateDto>>> GetAll(CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.ExchangeRates.Queries.GetExchangeRatesQuery(), cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExchangeRateDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.ExchangeRates.Queries.GetExchangeRateByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ExchangeRateDto>> Create(CreateExchangeRateDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.ExchangeRates.Commands.CreateExchangeRateCommand(TenantId, dto, CurrentUserId), cancellationToken));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExchangeRateDto>> Update(Guid id, UpdateExchangeRateDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.ExchangeRates.Commands.UpdateExchangeRateCommand(id, dto), cancellationToken));
    }

    [HttpPost("sync")]
    public async Task<ActionResult<int>> Sync(CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.ExchangeRates.Commands.SyncExchangeRatesCommand(TenantId, CurrentUserId), cancellationToken));
    }

    [HttpGet("external")]
    public async Task<ActionResult<ExternalRateDto>> FetchExternal([FromQuery] DateTime date, [FromQuery] string code, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.ExchangeRates.Queries.GetExternalRateQuery(date, code), cancellationToken));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.ExchangeRates.Commands.DeleteExchangeRateCommand(id), cancellationToken));
    }
}
