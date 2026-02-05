
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.SharedKernel.DTOs;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

//[Authorize]
[Route("api")]
public class ProductVariantsController : BaseApiController
{
    private readonly IProductVariantService _service;

    public ProductVariantsController(IProductVariantService service)
    {
        _service = service;
    }

    [HttpGet("products/{productId}/variants")]
    public async Task<ActionResult<IEnumerable<ProductVariantDto>>> GetByProduct(Guid productId, CancellationToken ct)
    {
        var result = await _service.GetByProductIdAsync(productId, ct);
        if (!result.IsSuccess) return BadRequest(new { success = false, message = result.Error });
        
        return Ok(new { success = true, data = result.Value });
    }

    [HttpGet("variants/{id}")]
    public async Task<ActionResult<ProductVariantDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return NotFound(new { success = false, message = result.Error });
        return Ok(new { success = true, data = result.Value });
    }

    [HttpPost("variants")]
    public async Task<ActionResult<ProductVariantDto>> Create(CreateProductVariantDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, TenantId, CurrentUserId, ct);
        if (!result.IsSuccess) return BadRequest(new { success = false, message = result.Error });
        
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, new { success = true, data = result.Value });
    }

    [HttpPut("variants/{id}")]
    public async Task<ActionResult<ProductVariantDto>> Update(Guid id, UpdateProductVariantDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, CurrentUserId, ct);
        if (!result.IsSuccess) return BadRequest(new { success = false, message = result.Error });
        return Ok(new { success = true, data = result.Value });
    }

    [HttpDelete("variants/{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, CurrentUserId, ct);
        if (!result.IsSuccess) return BadRequest(new { success = false, message = result.Error });
        return Ok(new { success = true, message = "Variant deleted" });
    }
}
