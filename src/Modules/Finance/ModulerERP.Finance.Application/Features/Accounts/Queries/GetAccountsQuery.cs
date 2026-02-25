using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.Accounts.Queries;

public record GetAccountsQuery() : IRequest<Result<List<AccountDto>>>;

public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, Result<List<AccountDto>>>
{
    private readonly IRepository<Account> _repository;

    public GetAccountsQueryHandler(IRepository<Account> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<AccountDto>>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _repository.GetAllAsync(cancellationToken);
        var accountDict = accounts.ToDictionary(a => a.Id, a => a.Name);

        var dtos = accounts.Select(a => new AccountDto
        {
            Id = a.Id,
            Code = a.Code,
            Name = a.Name,
            Description = a.Description,
            Type = a.Type.ToString(),
            ParentAccountId = a.ParentAccountId,
            ParentAccountName = a.ParentAccountId.HasValue && accountDict.ContainsKey(a.ParentAccountId.Value) ? accountDict[a.ParentAccountId.Value] : null,
            IsHeader = a.IsHeader,
            IsBankAccount = a.IsBankAccount,
            Balance = a.Balance,
            IsActive = a.IsActive
        }).OrderBy(a => a.Code).ToList();

        return Result<List<AccountDto>>.Success(dtos);
    }
}
