using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

//[Authorize]
[Route("api/product-categories")]
public class ProductCategoriesController : BaseApiController
{
    private readonly IProductCategoryService _service;

    public ProductCategoriesController(IProductCategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductCategoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(TenantId, cancellationToken);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductCategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, TenantId, cancellationToken);
        if (result == null)
            return NotFound(new { success = false, message = "Category not found" });
        
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<ActionResult<ProductCategoryDto>> Create(CreateProductCategoryDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, TenantId, CurrentUserId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductCategoryDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _service.UpdateAsync(id, dto, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, message = "Category updated successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Category not found" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(id, TenantId, CurrentUserId, cancellationToken);
            return Ok(new { success = true, message = "Category deleted successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Category not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
