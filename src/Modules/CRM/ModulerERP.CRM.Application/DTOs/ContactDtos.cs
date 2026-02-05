namespace ModulerERP.CRM.Application.DTOs;

// Contact DTOs
public record ContactListDto(
    Guid Id,
    Guid PartnerId,
    string FirstName,
    string LastName,
    string FullName,
    string? Position,
    string? Email,
    string? Phone,
    bool IsPrimary,
    bool IsActive,
    DateTime CreatedAt);

public record ContactDetailDto(
    Guid Id,
    Guid PartnerId,
    string PartnerName,
    string FirstName,
    string LastName,
    string? Position,
    string? Email,
    string? Phone,
    bool IsPrimary,
    bool IsActive,
    DateTime CreatedAt);

public record CreateContactDto(
    Guid PartnerId,
    string FirstName,
    string LastName,
    string? Position = null,
    string? Email = null,
    string? Phone = null,
    bool IsPrimary = false);

public record UpdateContactDto(
    string FirstName,
    string LastName,
    string? Position = null,
    string? Email = null,
    string? Phone = null,
    bool? IsPrimary = null,
    bool? IsActive = null);
