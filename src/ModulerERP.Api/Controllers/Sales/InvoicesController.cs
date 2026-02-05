using Microsoft.AspNetCore.Mvc;
using ModulerERP.Sales.Application.DTOs;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Api.Controllers.Sales;

[ApiController]
[Route("api/sales/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    public async Task<ActionResult<Result<List<InvoiceDto>>>> GetAll()
    {
        var result = await _invoiceService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<InvoiceDto>>> GetById(Guid id)
    {
        var result = await _invoiceService.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> Create([FromBody] CreateInvoiceDto dto)
    {
        var result = await _invoiceService.CreateAsync(dto);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result>> Update(Guid id, [FromBody] UpdateInvoiceDto dto)
    {
        var result = await _invoiceService.UpdateAsync(id, dto);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> Delete(Guid id)
    {
        var result = await _invoiceService.DeleteAsync(id);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id}/issue")]
    public async Task<ActionResult<Result>> Issue(Guid id)
    {
        var result = await _invoiceService.IssueAsync(id);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<Result>> Cancel(Guid id)
    {
        var result = await _invoiceService.CancelAsync(id);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}
