using ModulerERP.Sales.Domain.Enums;

namespace ModulerERP.Sales.Application.DTOs;

public record QuoteDto(
    Guid Id,
    string QuoteNumber,
    int RevisionNumber,
    Guid PartnerId,
    string PartnerName, // Enriched
    Guid? OpportunityId,
    QuoteStatus Status,
    Guid CurrencyId,
    string CurrencyCode, // Enriched
    decimal ExchangeRate,
    DateTime? SentDate,
    DateTime? ValidUntil,
    string? PaymentTerms,
    string? Notes,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    List<QuoteLineDto> Lines,
    DateTime CreatedAt,
    string CreatedByName
);

public record QuoteLineDto(
    Guid Id,
    Guid ProductId,
    string ProductName, // Enriched
    string Description,
    decimal Quantity,
    Guid UnitOfMeasureId,
    string UnitOfMeasureCode, // Enriched
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal DiscountAmount,
    decimal TaxPercent,
    decimal LineTotal
);

public record CreateQuoteDto(
    Guid PartnerId,
    Guid CurrencyId,
    decimal ExchangeRate,
    Guid? OpportunityId,
    DateTime? ValidUntil,
    string? PaymentTerms,
    string? Notes,
    List<CreateQuoteLineDto> Lines
);

public record CreateQuoteLineDto(
    Guid ProductId,
    string Description, // Can default to product name
    decimal Quantity,
    Guid UnitOfMeasureId,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal TaxPercent
);

public record UpdateQuoteDto(
    DateTime? ValidUntil,
    string? PaymentTerms,
    string? Notes,
    List<CreateQuoteLineDto> Lines
);
