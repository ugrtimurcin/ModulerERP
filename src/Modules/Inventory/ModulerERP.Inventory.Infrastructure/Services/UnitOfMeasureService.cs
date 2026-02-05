using Microsoft.EntityFrameworkCore;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;
using ModulerERP.Inventory.Domain.Entities;
using ModulerERP.Inventory.Infrastructure.Persistence;
using ModulerERP.Inventory.Domain.Enums;

namespace ModulerERP.Inventory.Infrastructure.Services;

public class UnitOfMeasureService : IUnitOfMeasureService
{
    private readonly InventoryDbContext _context;

    public UnitOfMeasureService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UnitOfMeasureDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var uoms = await _context.UnitOfMeasures
            .Where(u => u.TenantId == tenantId)
            .Include(u => u.BaseUnit)
            .OrderBy(u => u.Type)
            .ThenBy(u => u.Name)
            .ToListAsync(cancellationToken);

        return uoms.Select(MapToDto);
    }

    public async Task<IEnumerable<UnitOfMeasureDto>> GetByTypeAsync(UomType type, Guid tenantId, CancellationToken cancellationToken = default)
    {
         var uoms = await _context.UnitOfMeasures
            .Where(u => u.TenantId == tenantId && u.Type == type)
            .Include(u => u.BaseUnit)
            .OrderBy(u => u.ConversionFactor)
            .ToListAsync(cancellationToken);

        return uoms.Select(MapToDto);
    }

    public async Task<UnitOfMeasureDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var uom = await _context.UnitOfMeasures
            .Include(u => u.BaseUnit)
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, cancellationToken);

        return uom == null ? null : MapToDto(uom);
    }

    public async Task<UnitOfMeasureDto> CreateAsync(CreateUnitOfMeasureDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Unique Code check
        if (await _context.UnitOfMeasures.AnyAsync(u => u.TenantId == tenantId && u.Code == dto.Code, cancellationToken))
            throw new InvalidOperationException($"UOM with code '{dto.Code}' already exists.");

        // Validation: Base Unit logic
        if (dto.BaseUnitId.HasValue)
        {
            var baseUnit = await _context.UnitOfMeasures
                .FirstOrDefaultAsync(u => u.Id == dto.BaseUnitId && u.TenantId == tenantId, cancellationToken);
            
            if (baseUnit == null)
                throw new ArgumentException("Referenced Base Unit not found.");
            
            if (baseUnit.Type != dto.Type)
                throw new ArgumentException($"Base Unit must be of type {dto.Type}.");
        }

        var uom = UnitOfMeasure.Create(
            tenantId,
            dto.Code,
            dto.Name,
            dto.Type,
            userId,
            dto.ConversionFactor,
            dto.BaseUnitId);

        _context.UnitOfMeasures.Add(uom);
        await _context.SaveChangesAsync(cancellationToken);

        // Load reference for DTO
        if (dto.BaseUnitId.HasValue)
        {
             await _context.Entry(uom).Reference(u => u.BaseUnit).LoadAsync(cancellationToken);
        }

        return MapToDto(uom);
    }

    public async Task UpdateAsync(Guid id, UpdateUnitOfMeasureDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var uom = await _context.UnitOfMeasures
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, cancellationToken);

        if (uom == null)
            throw new KeyNotFoundException($"UOM with ID {id} not found.");

        uom.Update(dto.Code, dto.Name, dto.Type, dto.ConversionFactor);
        
        // Handle IsActive if exposed
        if (dto.IsActive)
            uom.Activate();
        else
            uom.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var uom = await _context.UnitOfMeasures
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, cancellationToken);

        if (uom == null)
            throw new KeyNotFoundException($"UOM with ID {id} not found.");

        // Check for dependencies: Products use this UOM
        var usedInProducts = await _context.Products.AnyAsync(p => p.UnitOfMeasureId == id && !p.IsDeleted, cancellationToken);
        if (usedInProducts)
            throw new InvalidOperationException("Cannot delete UOM used by products.");

        // Check for dependencies: Used as Base Unit
        var usedAsBase = await _context.UnitOfMeasures.AnyAsync(u => u.BaseUnitId == id && !u.IsDeleted, cancellationToken);
        if (usedAsBase)
            throw new InvalidOperationException("Cannot delete UOM used as a Base Unit for other units.");

        // Soft delete
        _context.UnitOfMeasures.Remove(uom);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static UnitOfMeasureDto MapToDto(UnitOfMeasure uom)
    {
        return new UnitOfMeasureDto
        {
            Id = uom.Id,
            Code = uom.Code,
            Name = uom.Name,
            Type = uom.Type,
            ConversionFactor = uom.ConversionFactor,
            BaseUnitId = uom.BaseUnitId,
            BaseUnitName = uom.BaseUnit?.Code, // Usually Code is more useful than Name for UOM
            IsActive = uom.IsActive
        };
    }
}
