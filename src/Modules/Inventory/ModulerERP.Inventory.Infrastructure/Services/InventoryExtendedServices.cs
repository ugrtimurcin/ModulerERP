using Microsoft.EntityFrameworkCore;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;
using ModulerERP.Inventory.Domain.Entities;
using ModulerERP.Inventory.Infrastructure.Persistence;

namespace ModulerERP.Inventory.Infrastructure.Services;

public class BrandService : IBrandService
{
    private readonly InventoryDbContext _context;

    public BrandService(InventoryDbContext context) => _context = context;

    public async Task<IEnumerable<BrandDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _context.Brands
            .Where(b => b.TenantId == tenantId && !b.IsDeleted)
            .Select(b => new BrandDto(b.Id, b.Code, b.Name, b.Description, b.Website, b.LogoUrl, b.IsActive))
            .ToListAsync(ct);
    }

    public async Task<BrandDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var b = await _context.Brands.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);
        return b == null ? null : new BrandDto(b.Id, b.Code, b.Name, b.Description, b.Website, b.LogoUrl, b.IsActive);
    }

    public async Task<BrandDto> CreateAsync(Guid tenantId, CreateBrandDto dto, Guid userId, CancellationToken ct = default)
    {
        var brand = Brand.Create(tenantId, dto.Code, dto.Name, userId, dto.Description, dto.Website);
        _context.Brands.Add(brand);
        await _context.SaveChangesAsync(ct);
        return new BrandDto(brand.Id, brand.Code, brand.Name, brand.Description, brand.Website, brand.LogoUrl, brand.IsActive);
    }

    public async Task<BrandDto?> UpdateAsync(Guid tenantId, Guid id, UpdateBrandDto dto, Guid userId, CancellationToken ct = default)
    {
        var brand = await _context.Brands.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (brand == null) return null;
        brand.Update(dto.Name, dto.Description, dto.Website, dto.LogoUrl);
        await _context.SaveChangesAsync(ct);
        return new BrandDto(brand.Id, brand.Code, brand.Name, brand.Description, brand.Website, brand.LogoUrl, brand.IsActive);
    }

    public async Task<bool> DeleteAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default)
    {
        var brand = await _context.Brands.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (brand == null) return false;
        brand.Delete(userId);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}

public class UnitConversionService : IUnitConversionService
{
    private readonly InventoryDbContext _context;

    public UnitConversionService(InventoryDbContext context) => _context = context;

    public async Task<IEnumerable<UnitConversionDto>> GetAllAsync(Guid tenantId, Guid? productId = null, CancellationToken ct = default)
    {
        var query = _context.UnitConversions
            .Include(u => u.FromUom)
            .Include(u => u.ToUom)
            .Include(u => u.Product)
            .Where(u => u.TenantId == tenantId && !u.IsDeleted);

        if (productId.HasValue)
            query = query.Where(u => u.ProductId == productId || u.ProductId == null);

        return await query.Select(u => new UnitConversionDto(
            u.Id, u.FromUomId, u.FromUom!.Code, u.ToUomId, u.ToUom!.Code,
            u.ConversionFactor, u.ProductId, u.Product != null ? u.Product.Sku : null
        )).ToListAsync(ct);
    }

    public async Task<UnitConversionDto> CreateAsync(Guid tenantId, CreateUnitConversionDto dto, Guid userId, CancellationToken ct = default)
    {
        var conversion = UnitConversion.Create(tenantId, dto.FromUomId, dto.ToUomId, dto.ConversionFactor, userId, dto.ProductId);
        _context.UnitConversions.Add(conversion);
        await _context.SaveChangesAsync(ct);

        var fromUom = await _context.UnitOfMeasures.FindAsync([conversion.FromUomId], ct);
        var toUom = await _context.UnitOfMeasures.FindAsync([conversion.ToUomId], ct);
        return new UnitConversionDto(conversion.Id, conversion.FromUomId, fromUom?.Code, conversion.ToUomId, toUom?.Code, conversion.ConversionFactor, conversion.ProductId, null);
    }

    public async Task<bool> DeleteAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default)
    {
        var conv = await _context.UnitConversions.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (conv == null) return false;
        conv.Delete(userId);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<decimal> ConvertAsync(Guid tenantId, Guid fromUomId, Guid toUomId, decimal quantity, Guid? productId = null, CancellationToken ct = default)
    {
        if (fromUomId == toUomId) return quantity;

        // Try product-specific first, then global
        var conversion = await _context.UnitConversions
            .Where(u => u.TenantId == tenantId && !u.IsDeleted && u.FromUomId == fromUomId && u.ToUomId == toUomId)
            .OrderByDescending(u => u.ProductId == productId)
            .FirstOrDefaultAsync(ct);

        if (conversion != null)
            return quantity * conversion.ConversionFactor;

        // Try reverse
        var reverse = await _context.UnitConversions
            .Where(u => u.TenantId == tenantId && !u.IsDeleted && u.FromUomId == toUomId && u.ToUomId == fromUomId)
            .OrderByDescending(u => u.ProductId == productId)
            .FirstOrDefaultAsync(ct);

        if (reverse != null)
            return quantity / reverse.ConversionFactor;

        throw new InvalidOperationException($"No conversion found from {fromUomId} to {toUomId}");
    }
}

public class ProductSerialService : IProductSerialService
{
    private readonly InventoryDbContext _context;

    public ProductSerialService(InventoryDbContext context) => _context = context;

    public async Task<IEnumerable<ProductSerialDto>> GetByProductAsync(Guid tenantId, Guid productId, CancellationToken ct = default)
    {
        return await _context.ProductSerials
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.TenantId == tenantId && s.ProductId == productId && !s.IsDeleted)
            .Select(s => new ProductSerialDto(
                s.Id, s.ProductId, s.Product!.Sku, s.SerialNumber, (int)s.Status, s.Status.ToString(),
                s.WarehouseId, s.Warehouse != null ? s.Warehouse.Name : null, s.SupplierSerial, s.WarrantyExpiry
            )).ToListAsync(ct);
    }

    public async Task<ProductSerialDto?> GetBySerialNumberAsync(Guid tenantId, string serialNumber, CancellationToken ct = default)
    {
        var s = await _context.ProductSerials
            .Include(x => x.Product)
            .Include(x => x.Warehouse)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.SerialNumber == serialNumber.ToUpperInvariant() && !x.IsDeleted, ct);

        return s == null ? null : new ProductSerialDto(
            s.Id, s.ProductId, s.Product?.Sku, s.SerialNumber, (int)s.Status, s.Status.ToString(),
            s.WarehouseId, s.Warehouse?.Name, s.SupplierSerial, s.WarrantyExpiry
        );
    }

    public async Task<ProductSerialDto> CreateAsync(Guid tenantId, CreateProductSerialDto dto, Guid userId, CancellationToken ct = default)
    {
        var serial = ProductSerial.Create(tenantId, dto.ProductId, dto.SerialNumber, userId, dto.WarehouseId);
        if (dto.SupplierSerial != null) serial.SetSupplierSerial(dto.SupplierSerial);
        if (dto.WarrantyExpiry.HasValue) serial.SetWarranty(dto.WarrantyExpiry.Value);

        _context.ProductSerials.Add(serial);
        await _context.SaveChangesAsync(ct);

        return new ProductSerialDto(serial.Id, serial.ProductId, null, serial.SerialNumber, (int)serial.Status, serial.Status.ToString(), serial.WarehouseId, null, serial.SupplierSerial, serial.WarrantyExpiry);
    }

    public async Task<ProductSerialDto?> ReserveAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var serial = await _context.ProductSerials.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (serial == null) return null;
        serial.Reserve();
        await _context.SaveChangesAsync(ct);
        return new ProductSerialDto(serial.Id, serial.ProductId, null, serial.SerialNumber, (int)serial.Status, serial.Status.ToString(), serial.WarehouseId, null, serial.SupplierSerial, serial.WarrantyExpiry);
    }

    public async Task<ProductSerialDto?> SellAsync(Guid tenantId, Guid id, Guid salesOrderLineId, CancellationToken ct = default)
    {
        var serial = await _context.ProductSerials.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (serial == null) return null;
        serial.Sell(salesOrderLineId);
        await _context.SaveChangesAsync(ct);
        return new ProductSerialDto(serial.Id, serial.ProductId, null, serial.SerialNumber, (int)serial.Status, serial.Status.ToString(), serial.WarehouseId, null, serial.SupplierSerial, serial.WarrantyExpiry);
    }
}

public class ProductBatchService : IProductBatchService
{
    private readonly InventoryDbContext _context;

    public ProductBatchService(InventoryDbContext context) => _context = context;

    public async Task<IEnumerable<ProductBatchDto>> GetByProductAsync(Guid tenantId, Guid productId, CancellationToken ct = default)
    {
        return await _context.ProductBatches
            .Include(b => b.Product)
            .Include(b => b.Warehouse)
            .Where(b => b.TenantId == tenantId && b.ProductId == productId && !b.IsDeleted)
            .Select(b => new ProductBatchDto(
                b.Id, b.ProductId, b.Product!.Sku, b.BatchNumber, b.WarehouseId, b.Warehouse!.Name,
                b.Quantity, b.InitialQuantity, b.ManufactureDate, b.ExpiryDate, (int)b.Status, b.Status.ToString(), b.DaysUntilExpiry
            )).ToListAsync(ct);
    }

    public async Task<IEnumerable<ProductBatchDto>> GetExpiringAsync(Guid tenantId, int daysAhead = 30, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(daysAhead);
        return await _context.ProductBatches
            .Include(b => b.Product)
            .Include(b => b.Warehouse)
            .Where(b => b.TenantId == tenantId && !b.IsDeleted && b.ExpiryDate <= cutoff && b.Status == BatchStatus.Available)
            .OrderBy(b => b.ExpiryDate)
            .Select(b => new ProductBatchDto(
                b.Id, b.ProductId, b.Product!.Sku, b.BatchNumber, b.WarehouseId, b.Warehouse!.Name,
                b.Quantity, b.InitialQuantity, b.ManufactureDate, b.ExpiryDate, (int)b.Status, b.Status.ToString(), b.DaysUntilExpiry
            )).ToListAsync(ct);
    }

    public async Task<ProductBatchDto> CreateAsync(Guid tenantId, CreateProductBatchDto dto, Guid userId, CancellationToken ct = default)
    {
        var batch = ProductBatch.Create(tenantId, dto.ProductId, dto.BatchNumber, dto.WarehouseId, dto.Quantity, userId, dto.ManufactureDate, dto.ExpiryDate);
        _context.ProductBatches.Add(batch);
        await _context.SaveChangesAsync(ct);
        return new ProductBatchDto(batch.Id, batch.ProductId, null, batch.BatchNumber, batch.WarehouseId, null, batch.Quantity, batch.InitialQuantity, batch.ManufactureDate, batch.ExpiryDate, (int)batch.Status, batch.Status.ToString(), batch.DaysUntilExpiry);
    }

    public async Task<ProductBatchDto?> ConsumeAsync(Guid tenantId, Guid id, decimal quantity, CancellationToken ct = default)
    {
        var batch = await _context.ProductBatches.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (batch == null) return null;
        batch.Consume(quantity);
        await _context.SaveChangesAsync(ct);
        return new ProductBatchDto(batch.Id, batch.ProductId, null, batch.BatchNumber, batch.WarehouseId, null, batch.Quantity, batch.InitialQuantity, batch.ManufactureDate, batch.ExpiryDate, (int)batch.Status, batch.Status.ToString(), batch.DaysUntilExpiry);
    }

    public async Task<ProductBatchDto?> QuarantineAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var batch = await _context.ProductBatches.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (batch == null) return null;
        batch.Quarantine();
        await _context.SaveChangesAsync(ct);
        return new ProductBatchDto(batch.Id, batch.ProductId, null, batch.BatchNumber, batch.WarehouseId, null, batch.Quantity, batch.InitialQuantity, batch.ManufactureDate, batch.ExpiryDate, (int)batch.Status, batch.Status.ToString(), batch.DaysUntilExpiry);
    }

    public async Task<ProductBatchDto?> ReleaseAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var batch = await _context.ProductBatches.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (batch == null) return null;
        batch.Release();
        await _context.SaveChangesAsync(ct);
        return new ProductBatchDto(batch.Id, batch.ProductId, null, batch.BatchNumber, batch.WarehouseId, null, batch.Quantity, batch.InitialQuantity, batch.ManufactureDate, batch.ExpiryDate, (int)batch.Status, batch.Status.ToString(), batch.DaysUntilExpiry);
    }
}

public class AttributeService : IAttributeService
{
    private readonly InventoryDbContext _context;

    public AttributeService(InventoryDbContext context) => _context = context;

    public async Task<IEnumerable<AttributeDefinitionDto>> GetAllDefinitionsAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _context.AttributeDefinitions
            .Include(a => a.Values.Where(v => !v.IsDeleted))
            .Where(a => a.TenantId == tenantId && !a.IsDeleted)
            .OrderBy(a => a.SortOrder)
            .Select(a => new AttributeDefinitionDto(
                a.Id, a.Code, a.Name, (int)a.Type, a.Type.ToString(), a.SortOrder,
                a.Values.OrderBy(v => v.SortOrder).Select(v => new AttributeValueDto(v.Id, v.AttributeDefinitionId, v.Value, v.Code, v.SortOrder))
            )).ToListAsync(ct);
    }

    public async Task<AttributeDefinitionDto> CreateDefinitionAsync(Guid tenantId, CreateAttributeDefinitionDto dto, Guid userId, CancellationToken ct = default)
    {
        var attr = AttributeDefinition.Create(tenantId, dto.Code, dto.Name, (AttributeType)dto.Type, userId, dto.SortOrder);
        _context.AttributeDefinitions.Add(attr);
        await _context.SaveChangesAsync(ct);
        return new AttributeDefinitionDto(attr.Id, attr.Code, attr.Name, (int)attr.Type, attr.Type.ToString(), attr.SortOrder, []);
    }

    public async Task<AttributeValueDto> AddValueAsync(Guid tenantId, CreateAttributeValueDto dto, Guid userId, CancellationToken ct = default)
    {
        var value = AttributeValue.Create(tenantId, dto.AttributeDefinitionId, dto.Value, userId, dto.Code, dto.SortOrder);
        _context.AttributeValues.Add(value);
        await _context.SaveChangesAsync(ct);
        return new AttributeValueDto(value.Id, value.AttributeDefinitionId, value.Value, value.Code, value.SortOrder);
    }

    public async Task<bool> DeleteValueAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default)
    {
        var value = await _context.AttributeValues.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (value == null) return false;
        value.Delete(userId);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
