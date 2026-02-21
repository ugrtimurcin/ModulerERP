using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Infrastructure.Persistence;

namespace ModulerERP.CRM.Infrastructure.Features.Leads.Queries;

public record GetLeadByIdQuery(Guid Id) : IRequest<LeadDetailDto?>;

public class GetLeadByIdQueryHandler : IRequestHandler<GetLeadByIdQuery, LeadDetailDto?>
{
    private readonly CRMDbContext _context;

    public GetLeadByIdQueryHandler(CRMDbContext context)
    {
        _context = context;
    }

    public async Task<LeadDetailDto?> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (lead == null) return null;

        return new LeadDetailDto(
            lead.Id,
            lead.Title,
            lead.FirstName,
            lead.LastName,
            lead.Company,
            lead.Email,
            lead.Phone,
            lead.Status.ToString(),
            lead.Source,
            lead.AssignedUserId,
            null,
            lead.ConvertedPartnerId,
            lead.ConvertedAt,
            lead.TerritoryId,
            lead.RejectionReasonId,
            lead.IsMarketingConsentGiven,
            lead.ConsentDate,
            lead.ConsentSource,
            lead.IsActive,
            lead.CreatedAt);
    }
}
