using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.WorkShifts.Queries;

public record GetWorkShiftsQuery : IRequest<IEnumerable<WorkShiftDto>>;
