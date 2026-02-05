using ModulerERP.Sales.Application.DTOs;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Sales.Application.Interfaces;

public interface IQuoteService
{
    Task<Result<List<QuoteDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<QuoteDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAsync(CreateQuoteDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Guid id, UpdateQuoteDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> SendAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> AcceptAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> RejectAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> CreateRevisionAsync(Guid id, CancellationToken cancellationToken = default);
}
