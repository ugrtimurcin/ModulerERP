using Microsoft.AspNetCore.Mvc;
using ModulerERP.Manufacturing.Application.DTOs;
using ModulerERP.Manufacturing.Application.Interfaces;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillOfMaterialsController : ControllerBase
{
    private readonly IBomService _service;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _userId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public BillOfMaterialsController(IBomService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<BomListDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var result = await _service.GetBomsAsync(_tenantId, page, pageSize, search);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BomDetailDto>> GetById(Guid id)
    {
        var bom = await _service.GetBomByIdAsync(_tenantId, id);
        if (bom == null) return NotFound();
        return Ok(new { success = true, data = bom });
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<BomListDto>>> GetByProduct(Guid productId)
    {
        var boms = await _service.GetBomsByProductAsync(_tenantId, productId);
        return Ok(new { success = true, data = boms });
    }

    [HttpPost]
    public async Task<ActionResult<BomDetailDto>> Create([FromBody] CreateBomDto dto)
    {
        var result = await _service.CreateBomAsync(_tenantId, dto, _userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BomDetailDto>> Update(Guid id, [FromBody] UpdateBomDto dto)
    {
        try
        {
            var result = await _service.UpdateBomAsync(_tenantId, id, dto);
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
            await _service.DeleteBomAsync(_tenantId, id, _userId);
            return Ok(new { success = true, message = "BOM deleted successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // Components
    [HttpPost("{id}/components")]
    public async Task<ActionResult<BomComponentDto>> AddComponent(Guid id, [FromBody] CreateBomComponentDto dto)
    {
        try
        {
            var result = await _service.AddComponentAsync(_tenantId, id, dto, _userId);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("components/{componentId}")]
    public async Task<IActionResult> RemoveComponent(Guid componentId)
    {
        try
        {
            await _service.RemoveComponentAsync(_tenantId, componentId);
            return Ok(new { success = true, message = "Component removed" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
