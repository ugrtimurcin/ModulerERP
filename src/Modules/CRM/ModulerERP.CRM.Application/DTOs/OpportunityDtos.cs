namespace ModulerERP.CRM.Application.DTOs;

// ── Address sub-DTO for request/response ──
public record AddressDto(
    string? Street = null,
    string? District = null,
    string? City = null,
    string? ZipCode = null,
    string? Country = null,
    string? Block = null,
    string? Parcel = null);

// ── Opportunity DTOs ──

public record OpportunityListDto(
    Guid Id,
    string Title,
    Guid? PartnerId,
    string? PartnerName,
    Guid? LeadId,
    string? LeadName,
    decimal EstimatedValue,
    string? CurrencyCode,
    string Stage,
    int Probability,
    decimal WeightedValue,
    DateTime? ExpectedCloseDate,
    Guid? AssignedUserId,
    string? AssignedUserName,
    Guid? TerritoryId,
    bool IsActive,
    DateTime CreatedAt);

public record OpportunityDetailDto(
    Guid Id,
    string Title,
    Guid? LeadId,
    string? LeadName,
    Guid? PartnerId,
    string? PartnerName,
    decimal EstimatedValue,
    Guid? CurrencyId,
    string? CurrencyCode,
    string Stage,
    int Probability,
    decimal WeightedValue,
    DateTime? ExpectedCloseDate,
    Guid? AssignedUserId,
    string? AssignedUserName,
    Guid? TerritoryId,
    Guid? CompetitorId,
    Guid? LossReasonId,
    bool IsActive,
    DateTime CreatedAt);

public record CreateOpportunityDto(
    string Title,
    decimal EstimatedValue,
    string CurrencyCode = "TRY",
    Guid? LeadId = null,
    Guid? PartnerId = null,
    Guid? CurrencyId = null,
    string Stage = "Discovery",
    DateTime? ExpectedCloseDate = null,
    Guid? AssignedUserId = null,
    Guid? TerritoryId = null);

public record UpdateOpportunityDto(
    string Title,
    decimal EstimatedValue,
    string CurrencyCode = "TRY",
    Guid? PartnerId = null,
    Guid? CurrencyId = null,
    string? Stage = null,
    int? Probability = null,
    DateTime? ExpectedCloseDate = null,
    Guid? AssignedUserId = null,
    Guid? TerritoryId = null,
    Guid? CompetitorId = null,
    Guid? LossReasonId = null,
    bool? IsActive = null);
