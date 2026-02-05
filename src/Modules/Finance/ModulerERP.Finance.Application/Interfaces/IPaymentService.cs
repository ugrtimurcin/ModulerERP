using ModulerERP.Finance.Application.DTOs;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Finance.Application.Interfaces;

public interface IPaymentService
{
    Task<Result<List<PaymentDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> CreateAsync(CreatePaymentDto dto, Guid createdByUserId, CancellationToken cancellationToken = default);
}
