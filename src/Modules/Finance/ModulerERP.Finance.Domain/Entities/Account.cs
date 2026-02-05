using ModulerERP.SharedKernel.Entities;
using ModulerERP.Finance.Domain.Enums;

namespace ModulerERP.Finance.Domain.Entities;

/// <summary>
/// Chart of Accounts with hierarchical structure.
/// </summary>
public class Account : BaseEntity
{
    /// <summary>Account code (e.g., '100', '100.01')</summary>
    public string Code { get; private set; } = string.Empty;
    
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    
    public AccountType Type { get; private set; }
    
    /// <summary>For hierarchical accounts</summary>
    public Guid? ParentAccountId { get; private set; }
    
    /// <summary>Is this a header account (non-postable)?</summary>
    public bool IsHeader { get; private set; }
    
    /// <summary>Is this a bank/cash account?</summary>
    public bool IsBankAccount { get; private set; }
    
    /// <summary>Associated currency for bank accounts</summary>
    public Guid? CurrencyId { get; private set; }
    
    /// <summary>Current balance (cached)</summary>
    public decimal Balance { get; private set; }

    // Navigation
    public Account? ParentAccount { get; private set; }
    public ICollection<Account> ChildAccounts { get; private set; } = new List<Account>();
    public ICollection<JournalEntryLine> JournalLines { get; private set; } = new List<JournalEntryLine>();

    private Account() { } // EF Core

    public static Account Create(
        Guid tenantId,
        string code,
        string name,
        AccountType type,
        Guid createdByUserId,
        string? description = null,
        Guid? parentAccountId = null,
        bool isHeader = false,
        bool isBankAccount = false,
        Guid? currencyId = null)
    {
        var account = new Account
        {
            Code = code,
            Name = name,
            Type = type,
            Description = description,
            ParentAccountId = parentAccountId,
            IsHeader = isHeader,
            IsBankAccount = isBankAccount,
            CurrencyId = currencyId
        };

        account.SetTenant(tenantId);
        account.SetCreator(createdByUserId);
        return account;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void UpdateBalance(decimal balance) => Balance = balance;

    /// <summary>Determine if debit increases this account</summary>
    public bool DebitIncreases =>
        Type == AccountType.Asset || Type == AccountType.Expense;
}
