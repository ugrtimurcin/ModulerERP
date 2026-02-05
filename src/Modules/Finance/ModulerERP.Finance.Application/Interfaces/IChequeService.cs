using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Finance.Application.Interfaces;

public interface IChequeService
{
    Task<Result<List<ChequeDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<ChequeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ChequeDto>> CreateAsync(CreateChequeDto dto, Guid createdByUserId, CancellationToken cancellationToken = default);
    Task<Result<ChequeDto>> UpdateStatusAsync(UpdateChequeStatusDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<Result<List<ChequeHistory>>> GetHistoryAsync(Guid chequeId, CancellationToken cancellationToken = default);
}
