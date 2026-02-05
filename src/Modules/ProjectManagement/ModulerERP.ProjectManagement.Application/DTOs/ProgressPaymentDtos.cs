using ModulerERP.ProjectManagement.Domain.Enums;

namespace ModulerERP.ProjectManagement.Application.DTOs;

public record ProgressPaymentDto(
    Guid Id,
    Guid ProjectId,
    int PaymentNo,
    DateTime Date,
    decimal PreviousCumulativeAmount,
    decimal CurrentAmount,
    decimal RetentionRate,
    decimal RetentionAmount,
    decimal NetPayableAmount,
    ProgressPaymentStatus Status
);

public record CreateProgressPaymentDto(
    Guid ProjectId,
    DateTime Date,
    decimal CurrentAmount,
    decimal RetentionRate
);
