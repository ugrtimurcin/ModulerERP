using ModulerERP.SharedKernel.Results;

namespace ModulerERP.SharedKernel.Interfaces;

public interface IStockOperationsService
{
    Task<Result> DeductStockAsync(Guid tenantId, Guid productId, Guid warehouseId, decimal quantity, string referenceType, string referenceNumber, CancellationToken cancellationToken = default);
    Task<Result> AddStockAsync(Guid tenantId, Guid productId, Guid warehouseId, decimal quantity, string referenceType, string referenceNumber, decimal? unitCost = null, CancellationToken cancellationToken = default);
}
