using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Departments.Queries;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.Departments;

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto?>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetDepartmentByIdQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<DepartmentDto?> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        var d = await _context.Departments
            .AsNoTracking()
            .Include(d => d.Manager)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.TenantId == tenantId && !x.IsDeleted, cancellationToken);

        if (d == null) return null;

        return new DepartmentDto(
            d.Id,
            d.Name,
            d.Description,
            d.ManagerId,
            d.Manager != null ? $"{d.Manager.FirstName} {d.Manager.LastName}" : null,
            d.CreatedAt
        );
    }
}
