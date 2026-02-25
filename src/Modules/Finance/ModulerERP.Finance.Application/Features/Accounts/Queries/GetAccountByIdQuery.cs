using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.Accounts.Queries;

public record GetAccountByIdQuery(Guid Id) : IRequest<Result<AccountDto>>;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, Result<AccountDto>>
{
    private readonly IRepository<Account> _repository;

    public GetAccountByIdQueryHandler(IRepository<Account> repository)
    {
        _repository = repository;
    }

    public async Task<Result<AccountDto>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (account == null)
            return Result<AccountDto>.Failure("Account not found");

        var dto = new AccountDto
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
            dto.ParentAccountName = parent?.Name;
        }

        return Result<AccountDto>.Success(dto);
    }
}
