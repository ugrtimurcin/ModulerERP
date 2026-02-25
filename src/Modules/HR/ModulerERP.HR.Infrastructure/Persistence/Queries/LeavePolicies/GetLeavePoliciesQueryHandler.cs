using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.Features.LeavePolicies;
using ModulerERP.HR.Infrastructure.Persistence;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.LeavePolicies;

public class GetLeavePoliciesQueryHandler : IRequestHandler<GetLeavePoliciesQuery, List<LeavePolicyDto>>
{
    private readonly HRDbContext _context;

    public GetLeavePoliciesQueryHandler(HRDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeavePolicyDto>> Handle(GetLeavePoliciesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Set<ModulerERP.HR.Domain.Entities.LeavePolicy>()
            .Select(p => new LeavePolicyDto(p.Id, p.Name, p.IsPaid, p.RequiresSgkMissingDayCode, p.DefaultDays, p.IsActive))
            .ToListAsync(cancellationToken);
    }
}
