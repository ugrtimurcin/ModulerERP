namespace ModulerERP.CRM.Application.DTOs;

// SupportTicket DTOs
public record SupportTicketListDto(
    Guid Id,
    string Title,
    int Priority,
    string PriorityName,
    int Status,
    string StatusName,
    Guid? PartnerId,
    string? PartnerName,
    Guid? AssignedUserId,
    DateTime CreatedAt);

public record SupportTicketDetailDto(
    Guid Id,
    string Title,
    string Description,
    int Priority,
    string PriorityName,
    int Status,
    string StatusName,
    Guid? PartnerId,
    string? PartnerName,
    Guid? AssignedUserId,
    string? Resolution,
    DateTime? ResolvedAt,
    DateTime? ClosedAt,
    DateTime CreatedAt);

public record CreateSupportTicketDto(
    string Title,
    string Description,
    int Priority = 1,
    Guid? PartnerId = null,
    Guid? AssignedUserId = null);

public record UpdateSupportTicketDto(
    string Title,
    string Description,
    int Priority,
    Guid? AssignedUserId = null);

public record ResolveTicketDto(string Resolution);
