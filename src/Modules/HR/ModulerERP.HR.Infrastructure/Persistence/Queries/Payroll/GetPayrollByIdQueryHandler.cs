using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Payroll.Queries; // Fixed Application Namespace
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.Payroll;

public class GetPayrollByIdQueryHandler : IRequestHandler<GetPayrollByIdQuery, PayrollDto?>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPayrollByIdQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PayrollDto?> Handle(GetPayrollByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        var p = await _context.Payrolls
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.TenantId == tenantId && !x.IsDeleted, cancellationToken);

        if (p == null) return null;

        return new PayrollDto(
            p.Id,
            p.Period,
            p.Description,
            p.Status,
            p.TotalAmount,
            p.CurrencyId,
            "TRY", // TODO: Fetch currency
            p.Entries.Count,
            p.CreatedAt
        );
    }
}
