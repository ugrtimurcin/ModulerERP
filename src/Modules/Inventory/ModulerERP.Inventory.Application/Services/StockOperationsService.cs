using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;
using ModulerERP.Inventory.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Inventory.Application.Services;

public class StockOperationsService : IStockOperationsService
{
    private readonly IStockService _stockService;
    private readonly ICurrentUserService _currentUserService;

    public StockOperationsService(IStockService stockService, ICurrentUserService currentUserService)
    {
        _stockService = stockService;
        _currentUserService = currentUserService;
    }

    public async Task<Result> AddStockAsync(Guid tenantId, Guid productId, Guid warehouseId, decimal quantity, string referenceType, string referenceNumber, decimal? unitCost = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = new CreateStockMovementDto
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = quantity,
                Type = MovementType.AdjustmentIn, // Or Purchase/Production based on Ref? 
                // For simplified Ops, we might need more specific types or map them.
                // Assuming AddStock is for generic addition. 
                // Actually, if we use this for Returns, we might need Type param?
                // Interface didn't have type. Let's assume generic IN/OUT or infer from RefType.
                // If RefType == "Purchase", use Purchase?
                // For now, let's map logically or assume AdjustmentIn if ambiguous.
                ReferenceType = referenceType,
                ReferenceNumber = referenceNumber,
                UnitCost = unitCost,
                MovementDate = DateTime.UtcNow
            };

            // Infer type
            if (referenceType == "PurchaseReceiver") dto.Type = MovementType.Purchase;
            else if (referenceType == "Production") dto.Type = MovementType.Production;
            else if (referenceType == "SalesReturn") dto.Type = MovementType.SalesReturn;
            else dto.Type = MovementType.AdjustmentIn;

            await _stockService.ProcessMovementAsync(dto, tenantId, _currentUserService.UserId, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DeductStockAsync(Guid tenantId, Guid productId, Guid warehouseId, decimal quantity, string referenceType, string referenceNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = new CreateStockMovementDto
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = quantity,
                Type = MovementType.AdjustmentOut, 
                ReferenceType = referenceType,
                ReferenceNumber = referenceNumber,
                MovementDate = DateTime.UtcNow
            };

            if (referenceType == "Shipment") dto.Type = MovementType.Sale;
            else if (referenceType == "Consumption") dto.Type = MovementType.Consumption;
            else if (referenceType == "PurchaseReturn") dto.Type = MovementType.PurchaseReturn;
            else dto.Type = MovementType.AdjustmentOut;

            await _stockService.ProcessMovementAsync(dto, tenantId, _currentUserService.UserId, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
