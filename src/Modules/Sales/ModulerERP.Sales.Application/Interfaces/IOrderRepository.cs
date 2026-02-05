using ModulerERP.Sales.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Sales.Application.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Order>> GetAllWithLinesAsync(CancellationToken cancellationToken = default);
    Task<int> GetNextOrderNumberAsync(CancellationToken cancellationToken = default);
}
