using ModulerERP.Finance.Application.DTOs;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Finance.Application.Interfaces;

public interface IAccountService
{
    Task<Result<List<AccountDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<AccountDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<AccountDto>> CreateAsync(CreateAccountDto dto, Guid createdByUserId, CancellationToken cancellationToken = default);
    Task<Result<AccountDto>> UpdateAsync(Guid id, UpdateAccountDto dto, CancellationToken cancellationToken = default);
}
