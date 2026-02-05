using ModulerERP.Sales.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Sales.Application.Interfaces;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Invoice>> GetAllWithLinesAsync(CancellationToken cancellationToken = default);
    Task<int> GetNextInvoiceNumberAsync(CancellationToken cancellationToken = default);
}
