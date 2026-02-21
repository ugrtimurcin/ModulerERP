using ModulerERP.CRM.Domain.Enums;

namespace ModulerERP.CRM.Application.DTOs;

public record ActivityDto(
    Guid Id,
    ActivityType Type,
    string Subject,
    string? Description,
    DateTime ActivityDate,
    Guid? LeadId,
    Guid? OpportunityId,
    Guid? PartnerId,
    bool IsScheduled,
    bool IsCompleted,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    Guid CreatedByUserId);

public record CreateActivityDto(
    ActivityType Type,
    string Subject,
    string? Description,
    DateTime ActivityDate,
    Guid? LeadId = null,
    Guid? OpportunityId = null,
    Guid? PartnerId = null,
    bool IsScheduled = false);

public record UpdateActivityDto(
    string Subject,
    string? Description,
    DateTime ActivityDate,
    bool? IsCompleted);
