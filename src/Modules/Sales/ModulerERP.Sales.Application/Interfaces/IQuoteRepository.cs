using ModulerERP.Sales.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Sales.Application.Interfaces;

public interface IQuoteRepository : IRepository<Quote>
{
    Task<Quote?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Quote>> GetAllWithLinesAsync(CancellationToken cancellationToken = default);
    Task<int> GetNextQuoteNumberAsync(CancellationToken cancellationToken = default);
}
