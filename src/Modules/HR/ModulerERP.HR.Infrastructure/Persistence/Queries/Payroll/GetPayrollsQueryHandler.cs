using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Payroll.Queries; // Fixed Application Namespace
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.Payroll;

public class GetPayrollsQueryHandler : IRequestHandler<GetPayrollsQuery, List<PayrollDto>>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPayrollsQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<PayrollDto>> Handle(GetPayrollsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        return await _context.Payrolls
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId && !p.IsDeleted && p.Period.StartsWith(request.Year.ToString()))
            .OrderByDescending(p => p.Period)
            .Select(p => new PayrollDto(
                p.Id,
                p.Period,
                p.Description,
                p.Status,
                p.TotalAmount,
                p.CurrencyId,
                "TRY", // TODO: Fetch actual currency code via join or service
                p.Entries.Count,
                p.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}
