using MediatR;
using ModulerERP.CRM.Application.DTOs;

namespace ModulerERP.CRM.Application.Features.Leads.Commands;

public record CreateLeadCommand(
    string FirstName,
    string LastName,
    string? Title = null,
    string? Company = null,
    string? Email = null,
    string? Phone = null,
    string? Source = null,
    Guid? AssignedUserId = null) : IRequest<LeadDetailDto>;
