using Microsoft.AspNetCore.Mvc;
using ModulerERP.Api.Controllers;
using ModulerERP.Procurement.Application.DTOs;
using ModulerERP.Procurement.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

[Route("api/procurement/rfqs")]
public class RfqsController : BaseApiController
{
    private readonly IRfqService _service;

    public RfqsController(IRfqService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<RequestForQuotationDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync(TenantId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RequestForQuotationDto>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(TenantId, id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RequestForQuotationDto>> Create(CreateRequestForQuotationDto dto)
    {
        return Ok(await _service.CreateAsync(TenantId, CurrentUserId, dto));
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(Guid id)
    {
        await _service.CloseAsync(TenantId, id);
        return Ok();
    }

    [HttpPost("{id}/award")]
    public async Task<IActionResult> Award(Guid id)
    {
        await _service.AwardAsync(TenantId, id);
        return Ok();
    }
}

[Route("api/procurement/quotes")]
public class PurchaseQuotesController : BaseApiController
{
    private readonly IPurchaseQuoteService _service;

    public PurchaseQuotesController(IPurchaseQuoteService service)
    {
        _service = service;
    }

    [HttpGet("by-rfq/{rfqId}")]
    public async Task<ActionResult<List<PurchaseQuoteDto>>> GetByRfq(Guid rfqId)
    {
        return Ok(await _service.GetByRfqIdAsync(TenantId, rfqId));
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseQuoteDto>> Create(CreatePurchaseQuoteDto dto)
    {
        return Ok(await _service.CreateAsync(TenantId, CurrentUserId, dto));
    }

    [HttpPost("{id}/accept")]
    public async Task<IActionResult> Accept(Guid id)
    {
        await _service.AcceptAsync(TenantId, id);
        return Ok();
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        await _service.RejectAsync(TenantId, id);
        return Ok();
    }
}

[Route("api/procurement/qc")]
public class QualityControlInspectionsController : BaseApiController
{
    private readonly IQcService _service;

    public QualityControlInspectionsController(IQcService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<QualityControlInspectionDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync(TenantId));
    }

    [HttpPost]
    public async Task<ActionResult<QualityControlInspectionDto>> Create(CreateQualityControlInspectionDto dto)
    {
        return Ok(await _service.CreateAsync(TenantId, CurrentUserId, dto));
    }
}

[Route("api/procurement/returns")]
public class PurchaseReturnsController : BaseApiController
{
    private readonly IPurchaseReturnService _service;

    public PurchaseReturnsController(IPurchaseReturnService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<PurchaseReturnDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync(TenantId));
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseReturnDto>> Create(CreatePurchaseReturnDto dto)
    {
        return Ok(await _service.CreateAsync(TenantId, CurrentUserId, dto));
    }

    [HttpPost("{id}/ship")]
    public async Task<IActionResult> Ship(Guid id)
    {
        await _service.ShipAsync(TenantId, id);
        return Ok();
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        await _service.CompleteAsync(TenantId, id);
        return Ok();
    }
}
