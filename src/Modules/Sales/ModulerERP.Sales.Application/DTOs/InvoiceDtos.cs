namespace ModulerERP.Sales.Application.DTOs;

using ModulerERP.Sales.Domain.Enums;

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid? OrderId,
    string? OrderNumber,
    Guid PartnerId,
    string PartnerName,
    InvoiceStatus Status,
    // Currency
    Guid CurrencyId,
    string CurrencyCode,
    decimal ExchangeRate,
    // Dates
    DateTime InvoiceDate,
    DateTime DueDate,
    // Addresses & Terms
    string? ShippingAddress,
    string? BillingAddress,
    string? PaymentTerms,
    string? Notes,
    // Totals
    decimal SubTotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal BalanceDue,
    // Lines
    List<InvoiceLineDto> Lines,
    DateTime CreatedAt,
    string CreatedBy
);

public record InvoiceLineDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Description,
    decimal Quantity,
    Guid UnitOfMeasureId,
    string UnitOfMeasureCode,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal DiscountAmount,
    decimal TaxPercent,
    decimal LineTotal
);

public record CreateInvoiceDto(
    Guid PartnerId,
    Guid CurrencyId,
    decimal ExchangeRate,
    Guid? OrderId,
    // Dates
    DateTime InvoiceDate,
    DateTime DueDate,
    // Terms
    string? PaymentTerms,
    string? ShippingAddress,
    string? BillingAddress,
    string? Notes,
    List<CreateInvoiceLineDto> Lines
);

public record CreateInvoiceLineDto(
    Guid ProductId,
    string Description,
    decimal Quantity,
    Guid UnitOfMeasureId,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal TaxPercent,
    Guid? OrderLineId // To link back to order line if needed
);

public record UpdateInvoiceDto(
    DateTime InvoiceDate,
    DateTime DueDate,
    string? PaymentTerms,
    string? ShippingAddress,
    string? BillingAddress,
    string? Notes,
    List<CreateInvoiceLineDto> Lines
);
