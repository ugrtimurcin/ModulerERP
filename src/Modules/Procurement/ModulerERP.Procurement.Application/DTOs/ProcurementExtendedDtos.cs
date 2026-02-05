using ModulerERP.Procurement.Domain.Entities;
using ModulerERP.Procurement.Domain.Enums;

namespace ModulerERP.Procurement.Application.DTOs;

// RFQs
public record RequestForQuotationDto(
    Guid Id,
    string RfqNumber,
    string Title,
    DateTime DeadLine,
    RfqStatus Status,
    List<RequestForQuotationItemDto> Items,
    List<PurchaseQuoteDto> Quotes
);

public record RequestForQuotationItemDto(
    Guid Id,
    Guid ProductId,
    decimal TargetQuantity
);

public record CreateRequestForQuotationDto(
    string Title,
    DateTime DeadLine,
    List<CreateRequestForQuotationItemDto> Items
);

public record CreateRequestForQuotationItemDto(
    Guid ProductId,
    decimal TargetQuantity
);

// Purchase Quotes
public record PurchaseQuoteDto(
    Guid Id,
    Guid RfqId,
    Guid SupplierId,
    string QuoteReference,
    DateTime ValidUntil,
    decimal TotalAmount,
    bool IsSelected,
    PurchaseQuoteStatus Status,
    List<PurchaseQuoteItemDto> Items
);

public record PurchaseQuoteItemDto(
    Guid Id,
    Guid ProductId,
    decimal Price,
    int LeadTimeDays
);

public record CreatePurchaseQuoteDto(
    Guid RfqId,
    Guid SupplierId,
    string QuoteReference,
    DateTime ValidUntil,
    decimal TotalAmount,
    List<CreatePurchaseQuoteItemDto> Items
);

public record CreatePurchaseQuoteItemDto(
    Guid RfqItemId,
    Guid ProductId,
    decimal Price,
    int LeadTimeDays
);

// QC Inspection
public record QualityControlInspectionDto(
    Guid Id,
    Guid ReceiptItemId,
    Guid InspectorId,
    DateTime InspectionDate,
    decimal QuantityPassed,
    decimal QuantityRejected,
    Guid TargetWarehouseId,
    Guid? RejectionReasonId,
    QualityControlStatus Status,
    string? Notes
);

public record CreateQualityControlInspectionDto(
    Guid ReceiptItemId,
    decimal QuantityPassed,
    decimal QuantityRejected,
    Guid TargetWarehouseId,
    Guid? RejectionReasonId,
    Guid? TargetLocationId,
    string? Notes
);

// Purchase Returns
public record PurchaseReturnDto(
    Guid Id,
    string ReturnNumber,
    Guid SupplierId,
    Guid GoodsReceiptId,
    PurchaseReturnStatus Status,
    List<PurchaseReturnItemDto> Items
);

public record PurchaseReturnItemDto(
    Guid Id,
    Guid ReceiptItemId,
    decimal Quantity,
    Guid? ReasonId
);

public record CreatePurchaseReturnDto(
    Guid SupplierId,
    Guid GoodsReceiptId,
    List<CreatePurchaseReturnItemDto> Items
);

public record CreatePurchaseReturnItemDto(
    Guid ReceiptItemId,
    decimal Quantity,
    Guid? ReasonId
);
