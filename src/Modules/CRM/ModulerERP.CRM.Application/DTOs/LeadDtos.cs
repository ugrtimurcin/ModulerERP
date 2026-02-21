namespace ModulerERP.CRM.Application.DTOs;

// Lead DTOs
public record LeadListDto(
    Guid Id,
    string? Title,
    string FirstName,
    string LastName,
    string FullName,
    string? Company,
    string? Email,
    string? Phone,
    string Status,
    string? Source,
    Guid? AssignedUserId,
    string? AssignedUserName,
    bool IsMarketingConsentGiven,
    Guid? TerritoryId,
    bool IsActive,
    DateTime CreatedAt);

public record LeadDetailDto(
    Guid Id,
    string? Title,
    string FirstName,
    string LastName,
    string? Company,
    string? Email,
    string? Phone,
    string Status,
    string? Source,
    Guid? AssignedUserId,
    string? AssignedUserName,
    Guid? ConvertedPartnerId,
    DateTime? ConvertedAt,
    Guid? TerritoryId,
    Guid? RejectionReasonId,
    bool IsMarketingConsentGiven,
    DateTime? ConsentDate,
    string? ConsentSource,
    bool IsActive,
    DateTime CreatedAt);

public record CreateLeadDto(
    string FirstName,
    string LastName,
    string? Title,
    string? Company = null,
    string? Email = null,
    string? Phone = null,
    string? Source = null,
    Guid? AssignedUserId = null,
    Guid? TerritoryId = null,
    bool IsMarketingConsentGiven = false,
    string? ConsentSource = null);

public record UpdateLeadDto(
    string FirstName,
    string LastName,
    string? Title,
    string? Company = null,
    string? Email = null,
    string? Phone = null,
    string? Source = null,
    string? Status = null,
    Guid? AssignedUserId = null,
    Guid? TerritoryId = null,
    Guid? RejectionReasonId = null,
    bool? IsMarketingConsentGiven = null,
    string? ConsentSource = null,
    bool? IsActive = null);
