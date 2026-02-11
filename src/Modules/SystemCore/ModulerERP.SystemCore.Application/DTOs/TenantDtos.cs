namespace ModulerERP.SystemCore.Application.DTOs;

public record TenantDto(
    Guid Id,
    string Name,
    string Subdomain,
    string? SubscriptionPlan,
    DateTime? SubscriptionExpiresAt,
    bool IsActive,
    Guid BaseCurrencyId,
    string TimeZone
);

public record TenantListDto(
    Guid Id,
    string Name,
    string Subdomain,
    string? SubscriptionPlan,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateTenantDto(
    string Name,
    string Subdomain,
    string AdminEmail,
    string AdminPassword,
    string AdminFirstName,
    string AdminLastName,
    Guid BaseCurrencyId,
    string? SubscriptionPlan = null
);

public record UpdateTenantDto(
    string Name,
    string? SubscriptionPlan,
    DateTime? SubscriptionExpiresAt,
    bool? IsActive
);
