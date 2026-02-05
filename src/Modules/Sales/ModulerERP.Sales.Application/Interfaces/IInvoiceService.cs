using ModulerERP.Sales.Application.DTOs;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Sales.Application.Interfaces;

public interface IInvoiceService
{
    Task<Result<List<InvoiceDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<InvoiceDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAsync(CreateInvoiceDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Guid id, UpdateInvoiceDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Actions
    Task<Result> IssueAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
