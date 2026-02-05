using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Domain.Enums;

namespace ModulerERP.Inventory.Application.Interfaces;

public interface IStockService
{
    Task<StockMovementDto> ProcessMovementAsync(CreateStockMovementDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovementDto>> ProcessTransferAsync(CreateStockTransferDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockLevelDto>> GetStockLevelsAsync(Guid tenantId, Guid? warehouseId = null, Guid? productId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovementDto>> GetMovementsAsync(Guid tenantId, Guid? warehouseId = null, Guid? productId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}
