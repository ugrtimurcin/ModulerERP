using ModulerERP.Sales.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Sales.Application.Interfaces;

public interface IShipmentRepository : IRepository<Shipment>
{
    Task<Shipment?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> GetNextShipmentNumberAsync(CancellationToken cancellationToken = default);
}
