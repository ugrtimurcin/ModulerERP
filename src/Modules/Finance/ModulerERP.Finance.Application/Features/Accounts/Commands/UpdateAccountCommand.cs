using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.Accounts.Commands;

public record UpdateAccountCommand(Guid Id, UpdateAccountDto Dto) : IRequest<Result<AccountDto>>;

public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, Result<AccountDto>>
{
    private readonly IRepository<Account> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAccountCommandHandler(IRepository<Account> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AccountDto>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (account == null)
             return Result<AccountDto>.Failure("Account not found");

        account.Update(request.Dto.Name, request.Dto.Description);
        if (request.Dto.IsActive && !account.IsActive) account.Activate();
        if (!request.Dto.IsActive && account.IsActive) account.Deactivate();

        _repository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedDto = new AccountDto
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
            updatedDto.ParentAccountName = parent?.Name;
        }

        return Result<AccountDto>.Success(updatedDto);
    }
}
