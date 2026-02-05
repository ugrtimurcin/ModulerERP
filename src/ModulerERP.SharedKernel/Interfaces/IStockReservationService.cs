using ModulerERP.SharedKernel.Results;

namespace ModulerERP.SharedKernel.Interfaces;

public interface IStockReservationService
{
    Task<Result> ReserveStockAsync(Guid tenantId, Guid productId, Guid warehouseId, decimal quantity, string referenceString, CancellationToken cancellationToken = default);
    Task<Result> ReleaseReservationAsync(Guid tenantId, Guid productId, Guid warehouseId, decimal quantity, string referenceString, CancellationToken cancellationToken = default);
}
