using ModulerERP.Finance.Domain.Enums;

namespace ModulerERP.Finance.Application.DTOs;

public class ChequeDto
{
    public Guid Id { get; set; }
    public string ChequeNumber { get; set; } = string.Empty;
    public ChequeType Type { get; set; }
    public string TypeName => Type.ToString();
    public string BankName { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string? AccountNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public ChequeStatus CurrentStatus { get; set; }
    public string StatusName => CurrentStatus.ToString();
    public Guid? CurrentLocationId { get; set; }
    public string? Drawer { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateChequeDto
{
    public string ChequeNumber { get; set; } = string.Empty;
    public int Type { get; set; } // 1=Own, 2=Customer
    public string BankName { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string? AccountNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public string? Drawer { get; set; }
}

public class UpdateChequeStatusDto
{
    public Guid ChequeId { get; set; }
    public int NewStatus { get; set; } // ChequeStatus enum
    public Guid? NewLocationId { get; set; } // Optional: PartnerId or AccountId
    public string? Description { get; set; } // For history
}
