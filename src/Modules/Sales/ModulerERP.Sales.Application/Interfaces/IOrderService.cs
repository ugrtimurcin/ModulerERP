using ModulerERP.Sales.Application.DTOs;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Sales.Application.Interfaces;

public interface IOrderService
{
    Task<Result<List<OrderDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAsync(CreateOrderDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Guid id, UpdateOrderDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<Result> ConfirmAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
