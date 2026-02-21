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
/// Detailed DTO for BusinessPartner (full fields including addresses).
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
    Guid? TerritoryId,
    Guid? DefaultCurrencyId,
    decimal DefaultDiscountRate,
    string? Website,
    string? Email,
    string? MobilePhone,
    string? Landline,
    string? Fax,
    string? WhatsappNumber,
    AddressDto? BillingAddress,
    AddressDto? ShippingAddress,
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
    Guid? TerritoryId = null,
    Guid? DefaultCurrencyId = null,
    string? TaxOffice = null,
    string? TaxNumber = null,
    string? IdentityNumber = null,
    string? Email = null,
    string? MobilePhone = null,
    AddressDto? BillingAddress = null,
    AddressDto? ShippingAddress = null);

/// <summary>
/// DTO for updating a BusinessPartner.
/// </summary>
public record UpdateBusinessPartnerDto(
    string Name,
    bool IsCustomer,
    bool IsSupplier,
    string Kind,
    Guid? GroupId = null,
    Guid? TerritoryId = null,
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
    decimal DefaultDiscountRate = 0,
    AddressDto? BillingAddress = null,
    AddressDto? ShippingAddress = null,
    bool? IsActive = null);
