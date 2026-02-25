using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.Features.EarningDeductionTypes;
using ModulerERP.HR.Infrastructure.Persistence;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.EarningDeductionTypes;

public class GetEarningDeductionTypesQueryHandler : IRequestHandler<GetEarningDeductionTypesQuery, List<EarningDeductionTypeDto>>
{
    private readonly HRDbContext _context;

    public GetEarningDeductionTypesQueryHandler(HRDbContext context)
    {
        _context = context;
    }

    public async Task<List<EarningDeductionTypeDto>> Handle(GetEarningDeductionTypesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Set<ModulerERP.HR.Domain.Entities.EarningDeductionType>()
            .Select(p => new EarningDeductionTypeDto(p.Id, p.Name, p.Category, p.IsTaxable, p.IsSgkExempt, p.MaxExemptAmount, p.IsActive))
            .ToListAsync(cancellationToken);
    }
}
