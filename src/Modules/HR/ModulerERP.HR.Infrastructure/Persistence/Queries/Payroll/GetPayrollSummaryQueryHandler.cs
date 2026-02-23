using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Payroll.Queries; // Fixed Application Namespace
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.Payroll;

public class GetPayrollSummaryQueryHandler : IRequestHandler<GetPayrollSummaryQuery, PayrollSummaryDto>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPayrollSummaryQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PayrollSummaryDto> Handle(GetPayrollSummaryQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        var period = $"{request.Year}-{request.Month:D2}";

        // Try to find the payroll for the given year/month
        var payroll = await _context.Payrolls
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Period == period && p.TenantId == tenantId && !p.IsDeleted, cancellationToken);
        
        if (payroll == null)
        {
            return new PayrollSummaryDto(
                request.Year,
                request.Month,
                0, // TotalGross (Not stored on Payroll, would need sum)
                0, // TotalDeductions
                0, // TotalNet
                0, // EmployeeCount
                "TRY"
            );
        }

        // Count entries for this payroll
        var employeeCount = await _context.PayrollEntries
            .AsNoTracking()
            .CountAsync(e => e.PayrollId == payroll.Id && !e.IsDeleted, cancellationToken);

        // Sum values from entries
        var sums = await _context.PayrollEntries
             .AsNoTracking()
             .Where(e => e.PayrollId == payroll.Id && !e.IsDeleted)
             .GroupBy(e => e.PayrollId)
             .Select(g => new 
             {
                 Gross = g.Sum(e => e.BaseSalary + e.TotalTaxableEarnings + e.TotalSgkExemptEarnings),
                 Deductions = g.Sum(e => e.SocialSecurityEmployee + e.ProvidentFundEmployee + e.IncomeTax + e.StampTax + e.PersonalAllowanceDeduction),
                 Net = g.Sum(e => e.NetPayable)
             })
             .FirstOrDefaultAsync(cancellationToken);

        return new PayrollSummaryDto(
            request.Year,
            request.Month,
            sums?.Gross ?? 0,
            sums?.Deductions ?? 0,
            sums?.Net ?? 0,
            employeeCount,
            "TRY" // Placeholder
        );
    }
}
