using ModulerERP.Finance.Domain.Enums;

namespace ModulerERP.Finance.Application.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty; // Enum string
    public string Method { get; set; } = string.Empty; // Enum string
    public Guid PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty; // Lookup
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty; // From CurrencyId
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty; // Bank/Cash Account
    public DateTime PaymentDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Status { get; set; } = "Posted"; // Payment status usually posted immediately or pending clearing
}

public class CreatePaymentDto
{
    public PaymentDirection Direction { get; set; }
    public PaymentMethod Method { get; set; }
    public Guid PartnerId { get; set; }
    public Guid CurrencyId { get; set; } // Or Code
    public decimal Amount { get; set; }
    public Guid AccountId { get; set; } // The Bank/Cash GL Account
    public DateTime PaymentDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
