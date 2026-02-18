using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Payroll.Queries; // Fixed Application Namespace
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.Payroll;

public class GetPayrollEntriesQueryHandler : IRequestHandler<GetPayrollEntriesQuery, List<PayrollEntryDto>>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPayrollEntriesQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<PayrollEntryDto>> Handle(GetPayrollEntriesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        var entries = await _context.PayrollEntries
            .AsNoTracking()
            .Include(e => e.Employee)
            .ThenInclude(emp => emp.Department)
            .Where(e => e.PayrollId == request.PayrollId && e.TenantId == tenantId && !e.IsDeleted)
            .OrderBy(e => e.Employee.FirstName)
            .ToListAsync(cancellationToken);

        return entries.Select(e => new PayrollEntryDto(
            e.Id,
            e.PayrollId,
            e.EmployeeId,
            $"{e.Employee.FirstName} {e.Employee.LastName}",
            e.BaseSalary,
            e.OvertimePay,
            e.CommissionPay,
            e.Bonus,
            e.TransportationAllowance,
            e.AdvanceDeduction,
            e.SocialSecurityEmployee,
            e.ProvidentFundEmployee,
            e.IncomeTax,
            e.NetPayable
        )).ToList();
    }
}
