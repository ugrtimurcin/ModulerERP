using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

//[Authorize]
[Route("api/products")]
public class ProductsController : BaseApiController
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(TenantId, cancellationToken);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, TenantId, cancellationToken);
        if (result == null) return NotFound(new { success = false, message = "Product not found" });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateAsync(dto, TenantId, CurrentUserId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _service.UpdateAsync(id, dto, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, message = "Product updated successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Product not found" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(id, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, message = "Product deleted successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Product not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // Barcode Management Attributes
    [HttpPost("{id}/barcodes")]
    public async Task<IActionResult> AddBarcode(Guid id, CreateProductBarcodeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _service.AddBarcodeAsync(id, dto, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, message = "Barcode added successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Product not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("{id}/barcodes/{barcodeId}")]
    public async Task<IActionResult> RemoveBarcode(Guid id, Guid barcodeId, CancellationToken cancellationToken)
    {
        try
        {
            await _service.RemoveBarcodeAsync(id, barcodeId, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, message = "Barcode removed successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Product or Barcode not found" });
        }
    }

    [HttpPut("{id}/barcodes/{barcodeId}/primary")]
    public async Task<IActionResult> SetPrimaryBarcode(Guid id, Guid barcodeId, CancellationToken cancellationToken)
    {
        try
        {
            await _service.SetPrimaryBarcodeAsync(id, barcodeId, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, message = "Primary barcode set successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Product or Barcode not found" });
        }
    }

    // Price Management Attributes
    [HttpPost("{id}/prices")]
    public async Task<IActionResult> AddPrice(Guid id, CreateProductPriceDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _service.AddPriceAsync(id, dto, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, message = "Price added successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Product not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("{id}/prices/{priceId}")]
    public async Task<IActionResult> RemovePrice(Guid id, Guid priceId, CancellationToken cancellationToken)
    {
        try
        {
            await _service.RemovePriceAsync(id, priceId, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, message = "Price removed successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Product or Price not found" });
        }
    }
}
