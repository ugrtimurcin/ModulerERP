using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Departments.Queries;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.Departments;

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, List<DepartmentDto>>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetDepartmentsQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<DepartmentDto>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        return await _context.Departments
            .AsNoTracking()
            .Where(d => d.TenantId == tenantId && !d.IsDeleted)
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto(
                d.Id,
                d.Name,
                d.Description,
                d.ManagerId,
                d.Manager != null ? $"{d.Manager.FirstName} {d.Manager.LastName}" : null,
                d.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}
