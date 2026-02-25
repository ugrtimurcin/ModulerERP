using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.Accounts.Commands;

public record CreateAccountCommand(CreateAccountDto Dto, Guid UserId) : IRequest<Result<AccountDto>>;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result<AccountDto>>
{
    private readonly IRepository<Account> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateAccountCommandHandler(IRepository<Account> repository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AccountDto>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.FirstOrDefaultAsync(a => a.Code == request.Dto.Code, cancellationToken);
        if (existing != null)
            return Result<AccountDto>.Failure($"Account code '{request.Dto.Code}' already exists.");

        var tenantId = _currentUserService.TenantId;

        var account = Account.Create(
            tenantId,
            request.Dto.Code,
            request.Dto.Name,
            request.Dto.Type,
            request.UserId,
            request.Dto.Description,
            request.Dto.ParentAccountId,
            request.Dto.IsHeader,
            request.Dto.IsBankAccount,
            request.Dto.CurrencyId
        );

        await _repository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch to return complete DTO
        var createdDto = new AccountDto
        {
            Id = account.Id,
            Code = account.Code,
            Name = account.Name,
            Description = account.Description,
            Type = account.Type.ToString(),
            ParentAccountId = account.ParentAccountId,
            IsHeader = account.IsHeader,
            IsBankAccount = account.IsBankAccount,
            Balance = account.Balance,
            IsActive = account.IsActive
        };

        if (account.ParentAccountId.HasValue)
        {
            var parent = await _repository.GetByIdAsync(account.ParentAccountId.Value, cancellationToken);
            createdDto.ParentAccountName = parent?.Name;
        }

        return Result<AccountDto>.Success(createdDto);
    }
}
