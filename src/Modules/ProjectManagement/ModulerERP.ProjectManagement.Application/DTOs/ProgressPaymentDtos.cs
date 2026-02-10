using ModulerERP.ProjectManagement.Domain.Enums;

public record ProgressPaymentDto(
    Guid Id,
    Guid ProjectId,
    int PaymentNo,
    DateTime Date,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal GrossWorkAmount,
    decimal MaterialOnSiteAmount,
    decimal CumulativeTotalAmount,
    decimal PreviousCumulativeAmount,
    decimal PeriodDeltaAmount,
    decimal RetentionRate,
    decimal RetentionAmount,
    decimal WithholdingTaxRate,
    decimal WithholdingTaxAmount,
    decimal AdvanceDeductionAmount,
    decimal NetPayableAmount,
    bool IsExpense,
    ProgressPaymentStatus Status,
    List<ProgressPaymentDetailDto> Details
);

public record ProgressPaymentDetailDto(
    Guid Id,
    Guid ProgressPaymentId,
    Guid BillOfQuantitiesItemId,
    string ItemCode,
    string Description,
    decimal PreviousCumulativeQuantity,
    decimal CumulativeQuantity,
    decimal PeriodQuantity,
    decimal UnitPrice,
    decimal TotalAmount,
    decimal PeriodAmount
);

public record CreateProgressPaymentDto(
    Guid ProjectId,
    DateTime Date,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal MaterialOnSiteAmount,
    decimal AdvanceDeductionAmount,
    bool IsExpense
);

public record UpdateProgressPaymentDetailDto(
    Guid Id, 
    decimal CumulativeQuantity
);
