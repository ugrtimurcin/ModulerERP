using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.TaxRules.Queries;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.TaxRules;

public class GetTaxRulesQueryHandler : 
    IRequestHandler<GetTaxRulesQuery, IEnumerable<TaxRuleDto>>,
    IRequestHandler<GetTaxRuleByIdQuery, TaxRuleDto?>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetTaxRulesQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<TaxRuleDto>> Handle(GetTaxRulesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        
        return await _context.TaxRules
            .Where(r => r.TenantId == tenantId)
            .OrderBy(r => r.Order)
            .Select(r => new TaxRuleDto(
                r.Id,
                r.Name,
                r.LowerLimit,
                r.UpperLimit,
                r.Rate,
                r.Order,
                r.EffectiveFrom,
                r.EffectiveTo
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<TaxRuleDto?> Handle(GetTaxRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        return await _context.TaxRules
            .Where(r => r.Id == request.Id && r.TenantId == tenantId)
            .Select(r => new TaxRuleDto(
                r.Id,
                r.Name,
                r.LowerLimit,
                r.UpperLimit,
                r.Rate,
                r.Order,
                r.EffectiveFrom,
                r.EffectiveTo
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
