using ModulerERP.Finance.Domain.Enums;

namespace ModulerERP.Finance.Application.DTOs;

public class LedgerPostRequest
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    
    public TransactionType TransactionType { get; set; }
    public string? Category { get; set; }
    
    public DateTime EventDate { get; set; }
    
    public string SourceType { get; set; } = string.Empty;
    public Guid? SourceId { get; set; }
    public string SourceNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid BaseCurrencyId { get; set; }
    public Guid TransactionCurrencyId { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    
    public Guid? PartnerId { get; set; }
    public Guid? CostCenterId { get; set; }

    /// <summary>Raw operational amounts required by the engine mapping</summary>
    public List<LedgerPostAmount> Amounts { get; set; } = new();
}

public class LedgerPostAmount
{
    public PostingAccountRole Role { get; set; }
    
    public decimal TransactionAmount { get; set; }
    public decimal BaseAmount { get; set; }
    
    /// <summary>Is this amount naturally a Debit (true) or Credit (false)?</summary>
    public bool IsDebit { get; set; }
    
    public string? LineDescription { get; set; }
}
