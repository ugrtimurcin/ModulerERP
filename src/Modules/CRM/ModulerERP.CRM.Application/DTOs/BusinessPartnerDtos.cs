namespace ModulerERP.CRM.Application.DTOs;

/// <summary>
/// List DTO for BusinessPartner (minimal fields for table display).
/// </summary>
public record BusinessPartnerListDto(
    Guid Id,
    string Code,
    string Name,
    bool IsCustomer,
    bool IsSupplier,
    string? Email,
    string? MobilePhone,
    bool IsActive,
    DateTime CreatedAt);

/// <summary>
/// Detailed DTO for BusinessPartner (full fields).
/// </summary>
public record BusinessPartnerDetailDto(
    Guid Id,
    string Code,
    string Name,
    bool IsCustomer,
    bool IsSupplier,
    string Kind,
    string? TaxOffice,
    string? TaxNumber,
    string? IdentityNumber,
    Guid? GroupId,
    Guid? DefaultCurrencyId,
    int PaymentTermDays,
    decimal CreditLimit,
    decimal DefaultDiscountRate,
    string? Website,
    string? Email,
    string? MobilePhone,
    string? Landline,
    string? Fax,
    string? WhatsappNumber,
    bool IsActive,
    DateTime CreatedAt);

/// <summary>
/// DTO for creating a new BusinessPartner.
/// </summary>
public record CreateBusinessPartnerDto(
    string Code,
    string Name,
    bool IsCustomer,
    bool IsSupplier,
    string Kind,
    Guid? GroupId = null,
    Guid? DefaultCurrencyId = null,
    string? TaxOffice = null,
    string? TaxNumber = null,
    string? IdentityNumber = null,
    string? Email = null,
    string? MobilePhone = null);

/// <summary>
/// DTO for updating a BusinessPartner.
/// </summary>
public record UpdateBusinessPartnerDto(
    string Name,
    bool IsCustomer,
    bool IsSupplier,
    string Kind,
    Guid? GroupId = null,
    Guid? DefaultCurrencyId = null,
    string? TaxOffice = null,
    string? TaxNumber = null,
    string? IdentityNumber = null,
    string? Email = null,
    string? MobilePhone = null,
    string? Landline = null,
    string? Fax = null,
    string? WhatsappNumber = null,
    string? Website = null,
    int PaymentTermDays = 30,
    decimal CreditLimit = 0,
    decimal DefaultDiscountRate = 0,
    bool? IsActive = null);
