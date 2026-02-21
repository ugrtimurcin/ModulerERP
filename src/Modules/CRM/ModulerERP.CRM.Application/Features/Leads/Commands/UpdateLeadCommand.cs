using MediatR;
using ModulerERP.CRM.Application.DTOs;

namespace ModulerERP.CRM.Application.Features.Leads.Commands;

public record UpdateLeadCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? Title = null,
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
    bool? IsActive = null) : IRequest<LeadDetailDto>;
