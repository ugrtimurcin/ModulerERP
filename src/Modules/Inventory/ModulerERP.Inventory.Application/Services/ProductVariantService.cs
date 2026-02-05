
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;
using ModulerERP.Inventory.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Inventory.Application.Services;

public class ProductVariantService : IProductVariantService
{
    private readonly IRepository<ProductVariant> _variantRepository;
    private readonly IRepository<Product> _productRepository; // To verify parent exists
    private readonly IUnitOfWork _unitOfWork;

    public ProductVariantService(
        IRepository<ProductVariant> variantRepository,
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork)
    {
        _variantRepository = variantRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<ProductVariantDto>>> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        var variants = await _variantRepository.FindAsync(v => v.ProductId == productId, ct);
        
        var dtos = variants.Select(v => new ProductVariantDto(
            v.Id,
            v.ProductId,
            v.Code,
            v.Name,
            v.Attributes,
            0, // Stock level placeholder - fetched from StockService if needed
            v.ImageId,
            v.CreatedAt
        ));
        
        return Result<IEnumerable<ProductVariantDto>>.Success(dtos);
    }

    public async Task<Result<ProductVariantDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var v = await _variantRepository.GetByIdAsync(id, ct);
        if (v == null) return Result<ProductVariantDto>.Failure("Variant not found");

        var dto = new ProductVariantDto(
            v.Id,
            v.ProductId,
            v.Code,
            v.Name,
            v.Attributes,
            0, 
            v.ImageId,
            v.CreatedAt
        );
        return Result<ProductVariantDto>.Success(dto);
    }

    public async Task<Result<ProductVariantDto>> CreateAsync(CreateProductVariantDto dto, Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        // Check if product exists
        var product = await _productRepository.GetByIdAsync(dto.ProductId, ct);
        if (product == null) return Result<ProductVariantDto>.Failure("Parent product not found");

        // Check duplicate code
        var existing = await _variantRepository.FirstOrDefaultAsync(v => v.Code == dto.Code && v.TenantId == tenantId, ct);
        if (existing != null) return Result<ProductVariantDto>.Failure($"Variant code '{dto.Code}' already exists");

        var variant = ProductVariant.Create(
            tenantId,
            dto.ProductId,
            dto.Code,
            dto.Name,
            dto.Attributes,
            userId
        );

        await _variantRepository.AddAsync(variant, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(variant.Id, ct);
    }

    public async Task<Result<ProductVariantDto>> UpdateAsync(Guid id, UpdateProductVariantDto dto, Guid userId, CancellationToken ct = default)
    {
        var variant = await _variantRepository.GetByIdAsync(id, ct);
        if (variant == null) return Result<ProductVariantDto>.Failure("Variant not found");

        variant.Update(dto.Name, dto.Attributes);
        variant.SetUpdater(userId);

        _variantRepository.Update(variant);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<Result> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var variant = await _variantRepository.GetByIdAsync(id, ct);
        if (variant == null) return Result.Failure("Variant not found");

        // TODO: Check if involved in transactions/stock before deleting
        // For now, soft delete via repository if supported, or physical delete via standard repo
        
        _variantRepository.Remove(variant);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
