namespace ModulerERP.CRM.Application.DTOs;

// Opportunity DTOs
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
    bool IsActive,
    DateTime CreatedAt);

public record CreateOpportunityDto(
    string Title,
    decimal EstimatedValue,
    Guid? LeadId = null,
    Guid? PartnerId = null,
    Guid? CurrencyId = null,
    string Stage = "Discovery",
    DateTime? ExpectedCloseDate = null,
    Guid? AssignedUserId = null);

public record UpdateOpportunityDto(
    string Title,
    decimal EstimatedValue,
    Guid? PartnerId = null,
    Guid? CurrencyId = null,
    string? Stage = null,
    int? Probability = null,
    DateTime? ExpectedCloseDate = null,
    Guid? AssignedUserId = null,
    bool? IsActive = null);
