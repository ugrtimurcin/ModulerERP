using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.WorkShifts.Queries;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.WorkShifts;

public class GetWorkShiftsQueryHandler : 
    IRequestHandler<GetWorkShiftsQuery, IEnumerable<WorkShiftDto>>,
    IRequestHandler<GetWorkShiftByIdQuery, WorkShiftDto?>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetWorkShiftsQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<WorkShiftDto>> Handle(GetWorkShiftsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        
        return await _context.WorkShifts
            .Where(w => w.TenantId == tenantId)
            .OrderBy(w => w.Name)
            .Select(w => new WorkShiftDto(
                w.Id,
                w.Name,
                w.StartTime.ToString(@"hh\:mm"), // Assuming TimeSpan format
                w.EndTime.ToString(@"hh\:mm"),
                w.BreakMinutes
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkShiftDto?> Handle(GetWorkShiftByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        return await _context.WorkShifts
            .Where(w => w.Id == request.Id && w.TenantId == tenantId)
            .Select(w => new WorkShiftDto(
                w.Id,
                w.Name,
                w.StartTime.ToString(@"hh\:mm"),
                w.EndTime.ToString(@"hh\:mm"),
                w.BreakMinutes
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
