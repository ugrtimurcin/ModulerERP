using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.Features.SgkRiskProfiles;
using ModulerERP.HR.Infrastructure.Persistence;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.SgkRiskProfiles;

public class GetSgkRiskProfilesQueryHandler : IRequestHandler<GetSgkRiskProfilesQuery, List<SgkRiskProfileDto>>
{
    private readonly HRDbContext _context;

    public GetSgkRiskProfilesQueryHandler(HRDbContext context)
    {
        _context = context;
    }

    public async Task<List<SgkRiskProfileDto>> Handle(GetSgkRiskProfilesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Set<ModulerERP.HR.Domain.Entities.SgkRiskProfile>()
            .Select(p => new SgkRiskProfileDto(p.Id, p.Name, p.EmployerSgkMultiplier, p.Description))
            .ToListAsync(cancellationToken);
    }
}
