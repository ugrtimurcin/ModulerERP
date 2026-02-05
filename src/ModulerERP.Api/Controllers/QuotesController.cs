using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.Sales.Application.DTOs;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/sales/quotes")]
public class QuotesController : ControllerBase
{
    private readonly IQuoteService _quoteService;

    public QuotesController(IQuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    [HttpGet]
    public async Task<ActionResult<Result<List<QuoteDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _quoteService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<QuoteDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _quoteService.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> Create(CreateQuoteDto dto, CancellationToken cancellationToken)
    {
        var result = await _quoteService.CreateAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result>> Update(Guid id, UpdateQuoteDto dto, CancellationToken cancellationToken)
    {
        var result = await _quoteService.UpdateAsync(id, dto, cancellationToken);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _quoteService.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id}/send")]
    public async Task<ActionResult<Result>> Send(Guid id, CancellationToken cancellationToken)
    {
        var result = await _quoteService.SendAsync(id, cancellationToken);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id}/accept")]
    public async Task<ActionResult<Result>> Accept(Guid id, CancellationToken cancellationToken)
    {
        var result = await _quoteService.AcceptAsync(id, cancellationToken);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}
