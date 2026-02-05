using Microsoft.EntityFrameworkCore;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;
using ModulerERP.Inventory.Domain.Entities;
using ModulerERP.Inventory.Infrastructure.Persistence;

namespace ModulerERP.Inventory.Infrastructure.Services;

public class WarehouseService : IWarehouseService
{
    private readonly InventoryDbContext _context;

    public WarehouseService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var warehouses = await _context.Warehouses
            .Where(w => w.TenantId == tenantId)
            .OrderByDescending(w => w.IsDefault)
            .ThenBy(w => w.Name)
            .ToListAsync(cancellationToken);

        return warehouses.Select(MapToDto);
    }

    public async Task<WarehouseDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id && w.TenantId == tenantId, cancellationToken);

        return warehouse == null ? null : MapToDto(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Check code uniqueness
        if (await _context.Warehouses.AnyAsync(w => w.TenantId == tenantId && w.Code == dto.Code, cancellationToken))
            throw new InvalidOperationException($"Warehouse with code '{dto.Code}' already exists.");

        if (dto.IsDefault)
        {
            await UnsetPreviousDefault(tenantId, cancellationToken);
        }

        var warehouse = Warehouse.Create(
            tenantId,
            dto.Code,
            dto.Name,
            userId,
            dto.Description,
            dto.IsDefault,
            dto.BranchId);
            
        if (!string.IsNullOrEmpty(dto.Address))
            warehouse.SetAddress(dto.Address);

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(warehouse);
    }

    public async Task UpdateAsync(Guid id, UpdateWarehouseDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id && w.TenantId == tenantId, cancellationToken);

        if (warehouse == null)
            throw new KeyNotFoundException($"Warehouse with ID {id} not found.");

        if (dto.IsDefault && !warehouse.IsDefault)
        {
            await UnsetPreviousDefault(tenantId, cancellationToken);
            warehouse.SetAsDefault();
        }
        else if (!dto.IsDefault && warehouse.IsDefault)
        {
            warehouse.RemoveDefault();
        }

        warehouse.Update(dto.Name, dto.Description);
        
        if (!string.IsNullOrEmpty(dto.Address))
            warehouse.SetAddress(dto.Address);
            
        // Use domain methods for activation/deactivation if available, otherwise assume standard tracking
        if (dto.IsActive)
            warehouse.Activate();
        else
            warehouse.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses
            .Include(w => w.StockLevels)
            .FirstOrDefaultAsync(w => w.Id == id && w.TenantId == tenantId, cancellationToken);

        if (warehouse == null)
            throw new KeyNotFoundException($"Warehouse with ID {id} not found.");

        // Check for stock
        var hasStock = await _context.StockLevels.AnyAsync(s => s.WarehouseId == id && s.QuantityOnHand > 0, cancellationToken);
        if (hasStock)
            throw new InvalidOperationException("Cannot delete warehouse with existing stock.");

        // Soft delete
        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetDefaultAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id && w.TenantId == tenantId, cancellationToken);

        if (warehouse == null)
            throw new KeyNotFoundException($"Warehouse with ID {id} not found.");

        await UnsetPreviousDefault(tenantId, cancellationToken);
        
        warehouse.SetAsDefault();
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task UnsetPreviousDefault(Guid tenantId, CancellationToken cancellationToken)
    {
        var currentDefault = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.IsDefault, cancellationToken);
            
        if (currentDefault != null)
        {
            currentDefault.RemoveDefault();
        }
    }

    private static WarehouseDto MapToDto(Warehouse warehouse)
    {
        return new WarehouseDto
        {
            Id = warehouse.Id,
            Code = warehouse.Code,
            Name = warehouse.Name,
            Description = warehouse.Description,
            IsDefault = warehouse.IsDefault,
            BranchId = warehouse.BranchId,
            Address = warehouse.Address,
            IsActive = warehouse.IsActive
        };
    }
}
