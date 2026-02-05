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
    Guid? AssignedUserId = null);

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
    bool? IsActive = null);
