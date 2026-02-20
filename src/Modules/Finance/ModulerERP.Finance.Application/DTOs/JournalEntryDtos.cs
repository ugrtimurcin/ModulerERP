using ModulerERP.Finance.Domain.Enums;

namespace ModulerERP.Finance.Application.DTOs;

public class JournalEntryDto
{
    public Guid Id { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? SourceType { get; set; }
    public string? SourceNumber { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    
    public List<JournalEntryLineDto> Lines { get; set; } = new();
}

public class JournalEntryLineDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Debit { get; set; }
    public decimal? Credit { get; set; }
    public Guid? CurrencyId { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal? OriginalAmount { get; set; }
}

public class CreateJournalEntryDto
{
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public List<CreateJournalEntryLineDto> Lines { get; set; } = new();
}

public class CreateJournalEntryLineDto
{
    public Guid AccountId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public Guid? PartnerId { get; set; }
    public Guid? CurrencyId { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal? OriginalAmount { get; set; }
}
