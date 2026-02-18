using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.PublicHolidays.Queries;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.PublicHolidays;

public class GetPublicHolidaysQueryHandler : IRequestHandler<GetPublicHolidaysQuery, IReadOnlyList<PublicHolidayDto>>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPublicHolidaysQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<PublicHolidayDto>> Handle(GetPublicHolidaysQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        return await _context.PublicHolidays
            .Where(h => h.TenantId == tenantId)
            .OrderBy(h => h.Date)
            .Select(h => new PublicHolidayDto(h.Id, h.Date, h.Name, h.IsHalfDay))
            .ToListAsync(cancellationToken);
    }
}
