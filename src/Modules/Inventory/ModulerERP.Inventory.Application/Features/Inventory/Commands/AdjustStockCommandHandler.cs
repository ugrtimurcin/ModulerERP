using MediatR;
using ModulerERP.SharedKernel.Results;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.Inventory.Domain.Entities;
using ModulerERP.Inventory.Domain.Enums;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.SharedKernel.ValueObjects;

namespace ModulerERP.Inventory.Application.Features.Inventory.Commands;

public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, Result<Guid>>
{
    private readonly IRepository<StockMovement> _movementRepo;
    private readonly IRepository<StockLevel> _stockLevelRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AdjustStockCommandHandler(
        IRepository<StockMovement> movementRepo,
        IRepository<StockLevel> stockLevelRepo,
        IRepository<Product> productRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _movementRepo = movementRepo;
        _stockLevelRepo = stockLevelRepo;
        _productRepo = productRepo;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        var userId = _currentUserService.UserId;
        var dto = request.Dto;

        var product = await _productRepo.GetByIdAsync(dto.ProductId, cancellationToken);
        if (product == null || product.TenantId != tenantId)
            return Result<Guid>.Failure("Product not found.");

        // Create Movement
        // Determine type: AdjustmentIn (positive qty) or AdjustmentOut (negative qty)
        // Or trust dto.Type if it maps correctly.
        // Assuming user sends AdjustmentIn/Out explicitly.
        
        // If UnitCost provided, use it, else use Product Cost.
        var cost = dto.UnitCost.HasValue 
            ? Money.Create(dto.UnitCost.Value, "TRY") // Assuming TRY or need currency in DTO
            : product.CostPrice;

        var movement = StockMovement.Create(
            tenantId,
            dto.ProductId,
            dto.WarehouseId,
            dto.Type,
            dto.Quantity,
            userId,
            dto.LocationId,
            dto.ReferenceType,
            dto.ReferenceId,
            dto.ReferenceNumber,
            dto.UnitCost.HasValue ? ModulerERP.SharedKernel.ValueObjects.Money.Create(dto.UnitCost.Value, "TRY") : null,
            dto.Notes,
            dto.MovementDate
        );

        await _movementRepo.AddAsync(movement, cancellationToken);

        // Update Stock Level
        var stockLevel = await _stockLevelRepo.FirstOrDefaultAsync(l => l.WarehouseId == dto.WarehouseId && l.ProductId == dto.ProductId, cancellationToken);
        if (stockLevel == null)
        {
            if (dto.Quantity < 0) return Result<Guid>.Failure("Cannot reduce stock below zero (no stock level found).");
            
            stockLevel = StockLevel.Create(tenantId, dto.ProductId, dto.WarehouseId, dto.LocationId);
            await _stockLevelRepo.AddAsync(stockLevel, cancellationToken);
        }

        if (dto.Quantity > 0)
            stockLevel.AddStock(dto.Quantity);
        else
            stockLevel.RemoveStock(Math.Abs(dto.Quantity));
            
        _stockLevelRepo.Update(stockLevel);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(movement.Id);
    }
}
