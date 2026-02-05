using ModulerERP.Finance.Domain.Enums;

namespace ModulerERP.Finance.Application.DTOs;

public class AccountDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public bool IsHeader { get; set; }
    public bool IsBankAccount { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
}

public class CreateAccountDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string? Description { get; set; }
    public Guid? ParentAccountId { get; set; }
    public bool IsHeader { get; set; }
    public bool IsBankAccount { get; set; }
    public Guid? CurrencyId { get; set; }
}

public class UpdateAccountDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
