using ModulerERP.Sales.Domain.Enums;

namespace ModulerERP.Sales.Application.DTOs;

public record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid? QuoteId,
    Guid PartnerId,
    string PartnerName, // Enriched
    OrderStatus Status,
    Guid CurrencyId,
    string CurrencyCode, // Enriched
    decimal ExchangeRate,
    DateTime? RequestedDeliveryDate,
    string? ShippingAddress,
    string? BillingAddress,
    string? PaymentTerms,
    string? Notes,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    List<OrderLineDto> Lines,
    DateTime CreatedAt,
    string CreatedByName
);

public record OrderLineDto(
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
    decimal LineTotal,
    decimal ShippedQuantity,
    decimal InvoicedQuantity
);

public record CreateOrderDto(
    Guid PartnerId,
    Guid? WarehouseId,
    Guid CurrencyId,
    decimal ExchangeRate,
    Guid? QuoteId, // Optional conversion
    DateTime? RequestedDeliveryDate,
    string? ShippingAddress,
    string? BillingAddress,
    string? PaymentTerms,
    string? Notes,
    List<CreateOrderLineDto> Lines
);

public record CreateOrderLineDto(
    Guid ProductId,
    string Description,
    decimal Quantity,
    Guid UnitOfMeasureId,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal TaxPercent
);

public record UpdateOrderDto(
    DateTime? RequestedDeliveryDate,
    string? ShippingAddress,
    string? BillingAddress,
    string? PaymentTerms,
    string? Notes,
    List<CreateOrderLineDto> Lines
);
