using Microsoft.AspNetCore.Mvc;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Enums;


namespace ModulerERP.Api.Controllers;

public class FiscalPeriodsController : BaseApiController
{
    private readonly IFiscalPeriodService _service;

    public FiscalPeriodsController(IFiscalPeriodService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<FiscalPeriodDto>>> GetAll(CancellationToken cancellationToken)
    {
        return HandleResult(await _service.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FiscalPeriodDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<FiscalPeriodDto>> Create(CreateFiscalPeriodDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _service.CreateAsync(dto, CurrentUserId, cancellationToken));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FiscalPeriodDto>> Update(Guid id, UpdateFiscalPeriodDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _service.UpdateAsync(id, dto, cancellationToken));
    }

    [HttpPost("generate/{year}")]
    public async Task<ActionResult> Generate(int year, CancellationToken cancellationToken)
    {
        var result = await _service.GeneratePeriodsAsync(year, CurrentUserId, cancellationToken);
        if (result.IsSuccess) return Ok();
        return BadRequest(result.Error);
    }
}
