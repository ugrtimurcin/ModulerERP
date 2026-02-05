using ModulerERP.Inventory.Application.Interfaces;
using ModulerERP.Inventory.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System.Linq.Expressions;

namespace ModulerERP.Inventory.Application.Services;

public class StockReservationService : IStockReservationService
{
    private readonly IRepository<StockLevel> _stockLevelRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StockReservationService(
        IRepository<StockLevel> stockLevelRepository,
        IUnitOfWork unitOfWork)
    {
        _stockLevelRepository = stockLevelRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ReserveStockAsync(Guid tenantId, Guid productId, Guid warehouseId, decimal quantity, string referenceString, CancellationToken cancellationToken = default)
    {
        var stockLevel = await _stockLevelRepository.FirstOrDefaultAsync(
            sl => sl.ProductId == productId && sl.WarehouseId == warehouseId, 
            cancellationToken);

        if (stockLevel == null)
        {
            return Result.Failure($"Stock level not found for Product {productId} in Warehouse {warehouseId}");
        }

        try
        {
            stockLevel.Reserve(quantity);
            _stockLevelRepository.Update(stockLevel);
            await _unitOfWork.SaveChangesAsync(cancellationToken); 
            // Note: Saving changes here might be partial if part of larger transaction?
            // Since we are cross-module, this might run in its own scope or share scope if sharing DbContext?
            // Different Modules -> Different DbContexts -> Different Transactions (unless distributed transaction).
            // For now, we assume eventual consistency or happy path.
            
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message); // "Insufficient available stock..."
        }
        catch (Exception ex)
        {
            return Result.Failure($"Reservation failed: {ex.Message}");
        }
    }

    public async Task<Result> ReleaseReservationAsync(Guid tenantId, Guid productId, Guid warehouseId, decimal quantity, string referenceString, CancellationToken cancellationToken = default)
    {
        var stockLevel = await _stockLevelRepository.FirstOrDefaultAsync(
            sl => sl.ProductId == productId && sl.WarehouseId == warehouseId, 
            cancellationToken);

        if (stockLevel == null)
        {
             // If no stock level, strictly speaking nothing to release, but maybe we should warn?
             return Result.Failure($"Stock level not found for Product {productId} in Warehouse {warehouseId}");
        }

        stockLevel.ReleaseReservation(quantity);
        _stockLevelRepository.Update(stockLevel);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
