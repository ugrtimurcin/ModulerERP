using Microsoft.AspNetCore.Mvc;
using ModulerERP.Manufacturing.Application.DTOs;
using ModulerERP.Manufacturing.Application.Interfaces;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductionOrdersController : ControllerBase
{
    private readonly IProductionOrderService _service;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _userId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public ProductionOrdersController(IProductionOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductionOrderListDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? status = null)
    {
        var result = await _service.GetOrdersAsync(_tenantId, page, pageSize, status);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductionOrderDetailDto>> GetById(Guid id)
    {
        var order = await _service.GetOrderByIdAsync(_tenantId, id);
        if (order == null) return NotFound();
        return Ok(new { success = true, data = order });
    }

    [HttpPost]
    public async Task<ActionResult<ProductionOrderDetailDto>> Create([FromBody] CreateProductionOrderDto dto)
    {
        var result = await _service.CreateOrderAsync(_tenantId, dto, _userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductionOrderDetailDto>> Update(Guid id, [FromBody] UpdateProductionOrderDto dto)
    {
        try
        {
            var result = await _service.UpdateOrderAsync(_tenantId, id, dto);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _service.DeleteOrderAsync(_tenantId, id, _userId);
            return Ok(new { success = true, message = "Production order deleted" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // Lifecycle actions
    [HttpPost("{id}/plan")]
    public async Task<ActionResult<ProductionOrderDetailDto>> Plan(Guid id)
    {
        try
        {
            var result = await _service.PlanOrderAsync(_tenantId, id);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/release")]
    public async Task<ActionResult<ProductionOrderDetailDto>> Release(Guid id)
    {
        try
        {
            var result = await _service.ReleaseOrderAsync(_tenantId, id);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/start")]
    public async Task<ActionResult<ProductionOrderDetailDto>> Start(Guid id)
    {
        try
        {
            var result = await _service.StartOrderAsync(_tenantId, id);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<ProductionOrderDetailDto>> Complete(Guid id)
    {
        try
        {
            var result = await _service.CompleteOrderAsync(_tenantId, id);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ProductionOrderDetailDto>> Cancel(Guid id)
    {
        try
        {
            var result = await _service.CancelOrderAsync(_tenantId, id);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/record-production")]
    public async Task<ActionResult<ProductionOrderDetailDto>> RecordProduction(Guid id, [FromBody] RecordProductionDto dto)
    {
        try
        {
            var result = await _service.RecordProductionAsync(_tenantId, id, dto.Quantity);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
