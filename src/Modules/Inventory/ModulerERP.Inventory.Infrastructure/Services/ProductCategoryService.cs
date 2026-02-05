using Microsoft.EntityFrameworkCore;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;
using ModulerERP.Inventory.Domain.Entities;
using ModulerERP.Inventory.Infrastructure.Persistence;

namespace ModulerERP.Inventory.Infrastructure.Services;

public class ProductCategoryService : IProductCategoryService
{
    private readonly InventoryDbContext _context;

    public ProductCategoryService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductCategoryDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var categories = await _context.ProductCategories
            .Where(c => c.TenantId == tenantId)
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return categories.Select(MapToDto);
    }

    public async Task<ProductCategoryDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var category = await _context.ProductCategories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);

        return category == null ? null : MapToDto(category);
    }

    public async Task<ProductCategoryDto> CreateAsync(CreateProductCategoryDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var category = ProductCategory.Create(
            tenantId,
            dto.Name,
            userId,
            dto.Description,
            dto.ParentCategoryId,
            dto.SortOrder);

        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        // Load parent for the DTO if needed
        if (dto.ParentCategoryId.HasValue)
        {
            await _context.Entry(category).Reference(c => c.ParentCategory).LoadAsync(cancellationToken);
        }

        return MapToDto(category);
    }

    public async Task UpdateAsync(Guid id, UpdateProductCategoryDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);

        if (category == null)
            throw new KeyNotFoundException($"ProductCategory with ID {id} not found.");

        category.Update(dto.Name, dto.Description, dto.SortOrder);
        category.SetParentCategory(dto.ParentCategoryId);
        
        // Handle IsActive if exposed in method (DTO has it)
        if (dto.IsActive)
            category.Activate();
        else
            category.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);

        if (category == null)
            throw new KeyNotFoundException($"ProductCategory with ID {id} not found.");

        // Check for children
        var hasChildren = await _context.ProductCategories.AnyAsync(c => c.ParentCategoryId == id && !c.IsDeleted, cancellationToken);
        if (hasChildren)
            throw new InvalidOperationException("Cannot delete category with child categories.");

        // Check for products
        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id && !p.IsDeleted, cancellationToken);
        if (hasProducts)
             throw new InvalidOperationException("Cannot delete category with assigned products.");

        // Soft delete (BaseEntity via ISoftDeletable handled in DbContext or manually here)
        // DbContext.SaveChangesAsync handles ISoftDeletable logic if we set State = Deleted
        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static ProductCategoryDto MapToDto(ProductCategory category)
    {
        return new ProductCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive
        };
    }
}
