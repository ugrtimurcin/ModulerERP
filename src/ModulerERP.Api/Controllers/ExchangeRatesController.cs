using Microsoft.AspNetCore.Mvc;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Services;

namespace ModulerERP.Api.Controllers;

public class ExchangeRatesController : BaseApiController
{
    private readonly IExchangeRateService _service;

    public ExchangeRatesController(IExchangeRateService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<ExchangeRateDto>>> GetAll(CancellationToken cancellationToken)
    {
        return HandleResult(await _service.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExchangeRateDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ExchangeRateDto>> Create(CreateExchangeRateDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _service.CreateAsync(TenantId, dto, CurrentUserId, cancellationToken));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExchangeRateDto>> Update(Guid id, UpdateExchangeRateDto dto, CancellationToken cancellationToken)
    {
        // Note: Service currently returns failure as Entity update not implemented properly, 
        // but we exposed the endpoint.
        // Actually I updated the entity, so I should update the service to use it.
        // Let's do that quick fix on service next or leave as is if I didn't update service code yet.
        // I WILL update service code in next step to use the new UpdateRate method.
        return HandleResult(await _service.UpdateAsync(id, dto, cancellationToken));
    }

    [HttpPost("sync")]
    public async Task<ActionResult<int>> Sync(CancellationToken cancellationToken)
    {
        return HandleResult(await _service.SyncRatesAsync(TenantId, CurrentUserId, cancellationToken));
    }

    [HttpGet("external")]
    public async Task<ActionResult<ExternalRateDto>> FetchExternal([FromQuery] DateTime date, [FromQuery] string code, CancellationToken cancellationToken)
    {
        return HandleResult(await _service.GetExternalRateAsync(date, code, cancellationToken));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _service.DeleteAsync(id, cancellationToken));
    }
}
