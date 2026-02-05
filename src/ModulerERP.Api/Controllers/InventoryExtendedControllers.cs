using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly IBrandService _brandService;
    private readonly ICurrentUserService _currentUserService;

    public BrandsController(IBrandService brandService, ICurrentUserService currentUserService)
    {
        _brandService = brandService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BrandDto>>> GetAll()
    {
        var brands = await _brandService.GetAllAsync(_currentUserService.TenantId);
        return Ok(brands);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BrandDto>> GetById(Guid id)
    {
        var brand = await _brandService.GetByIdAsync(_currentUserService.TenantId, id);
        return brand == null ? NotFound() : Ok(brand);
    }

    [HttpPost]
    public async Task<ActionResult<BrandDto>> Create([FromBody] CreateBrandDto dto)
    {
        var brand = await _brandService.CreateAsync(_currentUserService.TenantId, dto, _currentUserService.UserId);
        return CreatedAtAction(nameof(GetById), new { id = brand.Id }, brand);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BrandDto>> Update(Guid id, [FromBody] UpdateBrandDto dto)
    {
        var brand = await _brandService.UpdateAsync(_currentUserService.TenantId, id, dto, _currentUserService.UserId);
        return brand == null ? NotFound() : Ok(brand);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _brandService.DeleteAsync(_currentUserService.TenantId, id, _currentUserService.UserId);
        return result ? NoContent() : NotFound();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitConversionsController : ControllerBase
{
    private readonly IUnitConversionService _service;
    private readonly ICurrentUserService _currentUserService;

    public UnitConversionsController(IUnitConversionService service, ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UnitConversionDto>>> GetAll([FromQuery] Guid? productId = null)
    {
        var conversions = await _service.GetAllAsync(_currentUserService.TenantId, productId);
        return Ok(conversions);
    }

    [HttpPost]
    public async Task<ActionResult<UnitConversionDto>> Create([FromBody] CreateUnitConversionDto dto)
    {
        var conversion = await _service.CreateAsync(_currentUserService.TenantId, dto, _currentUserService.UserId);
        return Ok(conversion);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(_currentUserService.TenantId, id, _currentUserService.UserId);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("convert")]
    public async Task<ActionResult<decimal>> Convert([FromQuery] Guid fromUomId, [FromQuery] Guid toUomId, [FromQuery] decimal quantity, [FromQuery] Guid? productId = null)
    {
        try
        {
            var result = await _service.ConvertAsync(_currentUserService.TenantId, fromUomId, toUomId, quantity, productId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

[ApiController]
[Route("api/products/{productId:guid}/serials")]
[Authorize]
public class ProductSerialsController : ControllerBase
{
    private readonly IProductSerialService _service;
    private readonly ICurrentUserService _currentUserService;

    public ProductSerialsController(IProductSerialService service, ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductSerialDto>>> GetByProduct(Guid productId)
    {
        var serials = await _service.GetByProductAsync(_currentUserService.TenantId, productId);
        return Ok(serials);
    }

    [HttpGet("/api/serials/{serialNumber}")]
    public async Task<ActionResult<ProductSerialDto>> GetBySerialNumber(string serialNumber)
    {
        var serial = await _service.GetBySerialNumberAsync(_currentUserService.TenantId, serialNumber);
        return serial == null ? NotFound() : Ok(serial);
    }

    [HttpPost]
    public async Task<ActionResult<ProductSerialDto>> Create(Guid productId, [FromBody] CreateProductSerialDto dto)
    {
        if (dto.ProductId != productId) return BadRequest("Product ID mismatch");
        var serial = await _service.CreateAsync(_currentUserService.TenantId, dto, _currentUserService.UserId);
        return Ok(serial);
    }

    [HttpPost("{id:guid}/reserve")]
    public async Task<ActionResult<ProductSerialDto>> Reserve(Guid id)
    {
        var serial = await _service.ReserveAsync(_currentUserService.TenantId, id);
        return serial == null ? NotFound() : Ok(serial);
    }
}

[ApiController]
[Route("api/products/{productId:guid}/batches")]
[Authorize]
public class ProductBatchesController : ControllerBase
{
    private readonly IProductBatchService _service;
    private readonly ICurrentUserService _currentUserService;

    public ProductBatchesController(IProductBatchService service, ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductBatchDto>>> GetByProduct(Guid productId)
    {
        var batches = await _service.GetByProductAsync(_currentUserService.TenantId, productId);
        return Ok(batches);
    }

    [HttpGet("/api/batches/expiring")]
    public async Task<ActionResult<IEnumerable<ProductBatchDto>>> GetExpiring([FromQuery] int daysAhead = 30)
    {
        var batches = await _service.GetExpiringAsync(_currentUserService.TenantId, daysAhead);
        return Ok(batches);
    }

    [HttpPost]
    public async Task<ActionResult<ProductBatchDto>> Create(Guid productId, [FromBody] CreateProductBatchDto dto)
    {
        if (dto.ProductId != productId) return BadRequest("Product ID mismatch");
        var batch = await _service.CreateAsync(_currentUserService.TenantId, dto, _currentUserService.UserId);
        return Ok(batch);
    }

    [HttpPost("{id:guid}/consume")]
    public async Task<ActionResult<ProductBatchDto>> Consume(Guid id, [FromQuery] decimal quantity)
    {
        var batch = await _service.ConsumeAsync(_currentUserService.TenantId, id, quantity);
        return batch == null ? NotFound() : Ok(batch);
    }

    [HttpPost("{id:guid}/quarantine")]
    public async Task<ActionResult<ProductBatchDto>> Quarantine(Guid id)
    {
        var batch = await _service.QuarantineAsync(_currentUserService.TenantId, id);
        return batch == null ? NotFound() : Ok(batch);
    }

    [HttpPost("{id:guid}/release")]
    public async Task<ActionResult<ProductBatchDto>> Release(Guid id)
    {
        var batch = await _service.ReleaseAsync(_currentUserService.TenantId, id);
        return batch == null ? NotFound() : Ok(batch);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttributesController : ControllerBase
{
    private readonly IAttributeService _service;
    private readonly ICurrentUserService _currentUserService;

    public AttributesController(IAttributeService service, ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttributeDefinitionDto>>> GetAll()
    {
        var attrs = await _service.GetAllDefinitionsAsync(_currentUserService.TenantId);
        return Ok(attrs);
    }

    [HttpPost]
    public async Task<ActionResult<AttributeDefinitionDto>> CreateDefinition([FromBody] CreateAttributeDefinitionDto dto)
    {
        var attr = await _service.CreateDefinitionAsync(_currentUserService.TenantId, dto, _currentUserService.UserId);
        return Ok(attr);
    }

    [HttpPost("values")]
    public async Task<ActionResult<AttributeValueDto>> AddValue([FromBody] CreateAttributeValueDto dto)
    {
        var value = await _service.AddValueAsync(_currentUserService.TenantId, dto, _currentUserService.UserId);
        return Ok(value);
    }

    [HttpDelete("values/{id:guid}")]
    public async Task<IActionResult> DeleteValue(Guid id)
    {
        var result = await _service.DeleteValueAsync(_currentUserService.TenantId, id, _currentUserService.UserId);
        return result ? NoContent() : NotFound();
    }
}
