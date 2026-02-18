using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Employees.Queries;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.Employees;

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto?>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetEmployeeByIdQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<EmployeeDto?> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        var e = await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Supervisor)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.TenantId == tenantId && !x.IsDeleted, cancellationToken);

        if (e == null) return null;

        return new EmployeeDto(
            e.Id,
            e.FirstName,
            e.LastName,
            e.Email,
            e.IdentityNumber,
            e.Citizenship,
            e.SocialSecurityType,
            e.WorkPermitNumber,
            e.WorkPermitExpDate,
            e.JobTitle,
            e.DepartmentId,
            e.Department?.Name ?? "Unknown",
            e.SupervisorId,
            e.Supervisor != null ? $"{e.Supervisor.FirstName} {e.Supervisor.LastName}" : null,
            e.CurrentSalary,
            e.CreatedAt,
            e.Status,
            e.QrToken,
            e.MaritalStatus,
            e.IsSpouseWorking,
            e.ChildCount,
            e.IsPensioner
        );
    }
}
