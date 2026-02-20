using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using ModulerERP.Finance.Domain.Enums;


namespace ModulerERP.Finance.Application.Services;

public class AccountService : IAccountService
{
    private readonly IRepository<Account> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AccountService(IRepository<Account> repository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<AccountDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Include ParentAccount if Repository supports Includes via spec or override
        // For now, fetching all.
        var accounts = await _repository.GetAllAsync(cancellationToken);
        
        // Manual mapping or improvement needed for Parent Name if excessive DB calls
        // Optimizing by fetching all to memory if list is small (Chart of Accounts usually < 1000)
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

    public async Task<Result<AccountDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _repository.GetByIdAsync(id, cancellationToken);
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

    public async Task<Result<AccountDto>> CreateAsync(CreateAccountDto dto, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        // Check for duplicate code
        var existing = await _repository.FirstOrDefaultAsync(a => a.Code == dto.Code, cancellationToken);
        if (existing != null)
            return Result<AccountDto>.Failure($"Account code '{dto.Code}' already exists.");

        // Create Entity
        // Note: TenantId should come from context/service, hardcoding or passing via method needed. 
        // For now Assuming AccountService is Scoped and we might need User Context accessor if tenant is strict.
        // Assuming TenantId is handled by Repository usually? No, Entity creation needs it. 
        // We will default to a known Tenant ID or TODO if Multi-tenant context not fully injected here.
        // Using System Admin or similar logic for now or Guid.Empty if ignored in current MVP phase.
        // Correction: AccountService doesn't have TenantId context here. 
        // Ideally should be passed in or injected ICurrentUserService.
        // Setting Guid.Empty for TenantId as placeholder consistent with other MVP services if not using RequestContext.
        var tenantId = _currentUserService.TenantId;

        var account = Account.Create(
            tenantId,
            dto.Code,
            dto.Name,
            dto.Type,
            createdByUserId,
            dto.Description,
            dto.ParentAccountId,
            dto.IsHeader,
            dto.IsBankAccount,
            dto.CurrencyId
        );

        await _repository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(account.Id, cancellationToken);
    }

    public async Task<Result<AccountDto>> UpdateAsync(Guid id, UpdateAccountDto dto, CancellationToken cancellationToken = default)
    {
        var account = await _repository.GetByIdAsync(id, cancellationToken);
        if (account == null)
             return Result<AccountDto>.Failure("Account not found");

        account.Update(dto.Name, dto.Description);
        if (dto.IsActive && !account.IsActive) account.Activate();
        if (!dto.IsActive && account.IsActive) account.Deactivate();

        _repository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }
}
