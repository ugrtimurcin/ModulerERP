using Microsoft.EntityFrameworkCore;
using ModulerERP.Manufacturing.Application.DTOs;
using ModulerERP.Manufacturing.Application.Interfaces;
using ModulerERP.Manufacturing.Domain.Entities;
using ModulerERP.Manufacturing.Domain.Enums;
using ModulerERP.Manufacturing.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Manufacturing.Infrastructure.Services;

public class BomService : IBomService
{
    private readonly ManufacturingDbContext _context;

    public BomService(ManufacturingDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<BomListDto>> GetBomsAsync(Guid tenantId, int page, int pageSize, string? search = null, CancellationToken ct = default)
    {
        var query = _context.BillOfMaterials
            .Include(b => b.Components)
            .Where(b => b.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.Code.Contains(search) || b.Name.Contains(search));

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var data = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BomListDto(
                b.Id,
                b.Code,
                b.Name,
                b.ProductId,
                null,
                b.Quantity,
                (int)b.Type,
                b.Type.ToString(),
                b.IsDefault,
                b.Components.Count,
                b.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<BomListDto>(data, page, pageSize, totalCount, totalPages);
    }

    public async Task<BomDetailDto?> GetBomByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var bom = await _context.BillOfMaterials
            .Include(b => b.Components)
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == id, ct);

        if (bom == null) return null;

        return MapToDetailDto(bom);
    }

    public async Task<IEnumerable<BomListDto>> GetBomsByProductAsync(Guid tenantId, Guid productId, CancellationToken ct = default)
    {
        return await _context.BillOfMaterials
            .Include(b => b.Components)
            .Where(b => b.TenantId == tenantId && b.ProductId == productId)
            .Select(b => new BomListDto(
                b.Id,
                b.Code,
                b.Name,
                b.ProductId,
                null,
                b.Quantity,
                (int)b.Type,
                b.Type.ToString(),
                b.IsDefault,
                b.Components.Count,
                b.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<BomDetailDto> CreateBomAsync(Guid tenantId, CreateBomDto dto, Guid userId, CancellationToken ct = default)
    {
        var bom = BillOfMaterials.Create(
            tenantId,
            dto.Code,
            dto.Name,
            dto.ProductId,
            dto.Quantity,
            userId,
            (BomType)dto.Type,
            dto.IsDefault);

        if (dto.EffectiveFrom.HasValue || dto.EffectiveTo.HasValue)
            bom.SetValidity(dto.EffectiveFrom, dto.EffectiveTo);

        _context.BillOfMaterials.Add(bom);
        await _context.SaveChangesAsync(ct);

        return (await GetBomByIdAsync(tenantId, bom.Id, ct))!;
    }

    public async Task<BomDetailDto> UpdateBomAsync(Guid tenantId, Guid id, UpdateBomDto dto, CancellationToken ct = default)
    {
        var bom = await _context.BillOfMaterials
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == id, ct)
            ?? throw new KeyNotFoundException("BOM not found");

        bom.SetValidity(dto.EffectiveFrom, dto.EffectiveTo);
        await _context.SaveChangesAsync(ct);

        return (await GetBomByIdAsync(tenantId, id, ct))!;
    }

    public async Task DeleteBomAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default)
    {
        var bom = await _context.BillOfMaterials
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == id, ct)
            ?? throw new KeyNotFoundException("BOM not found");

        bom.Delete(userId);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<BomComponentDto> AddComponentAsync(Guid tenantId, Guid bomId, CreateBomComponentDto dto, Guid userId, CancellationToken ct = default)
    {
        var bom = await _context.BillOfMaterials
            .Include(b => b.Components)
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == bomId, ct)
            ?? throw new KeyNotFoundException("BOM not found");

        var lineNumber = bom.Components.Count + 1;
        var component = BomComponent.Create(bomId, dto.ProductId, dto.Quantity, Guid.Empty, lineNumber);
        _context.BomComponents.Add(component);
        await _context.SaveChangesAsync(ct);

        return new BomComponentDto(component.Id, component.ProductId, null, component.Quantity, null, component.LineNumber);
    }

    public async Task RemoveComponentAsync(Guid tenantId, Guid componentId, CancellationToken ct = default)
    {
        var component = await _context.BomComponents
            .FirstOrDefaultAsync(c => c.Id == componentId, ct)
            ?? throw new KeyNotFoundException("Component not found");

        _context.BomComponents.Remove(component);
        await _context.SaveChangesAsync(ct);
    }

    private BomDetailDto MapToDetailDto(BillOfMaterials bom)
    {
        return new BomDetailDto(
            bom.Id,
            bom.Code,
            bom.Name,
            bom.ProductId,
            null,
            bom.Quantity,
            (int)bom.Type,
            bom.Type.ToString(),
            bom.IsDefault,
            bom.EffectiveFrom,
            bom.EffectiveTo,
            bom.Notes,
            bom.Components.Select(c => new BomComponentDto(c.Id, c.ProductId, null, c.Quantity, null, c.LineNumber)),
            bom.CreatedAt);
    }
}
