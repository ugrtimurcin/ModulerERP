using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Employees.Queries;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.Employees;

public class GetEmployeesLookupQueryHandler : IRequestHandler<GetEmployeesLookupQuery, List<EmployeeLookupDto>>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetEmployeesLookupQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<EmployeeLookupDto>> Handle(GetEmployeesLookupQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        return await _context.Employees
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId && !e.IsDeleted)
            .OrderBy(e => e.FirstName)
            .Select(e => new EmployeeLookupDto(e.Id, e.FirstName, e.LastName, e.JobTitle))
            .ToListAsync(cancellationToken);
    }
}
