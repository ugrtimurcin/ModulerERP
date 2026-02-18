using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Employees.Queries;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.Employees;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, List<EmployeeDto>>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetEmployeesQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        var query = _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Supervisor)
            .Where(e => e.TenantId == tenantId && !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .AsQueryable();

        // Pagination
        if (request.PageSize > 0)
        {
            query = query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize);
        }

        var employees = await query.ToListAsync(cancellationToken);

        return employees.Select(e => new EmployeeDto(
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
        )).ToList();
    }
}
