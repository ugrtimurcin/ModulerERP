using ModulerERP.ProjectManagement.Domain.Entities;

namespace ModulerERP.ProjectManagement.Application.DTOs;

public record ProjectChangeOrderDto(
    Guid Id,
    Guid ProjectId,
    int OrderNo,
    string Title,
    string Description,
    decimal AmountChange,
    int TimeExtensionDays,
    ChangeOrderStatus Status,
    DateTime RequestDate,
    DateTime? ApprovalDate,
    Guid? ApproverId
);

public record CreateChangeOrderDto(
    Guid ProjectId,
    string Title,
    string Description,
    decimal AmountChange,
    int TimeExtensionDays
);

public record UpdateChangeOrderStatusDto(
    Guid Id,
    ChangeOrderStatus Status
);
