using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.WorkShifts.Commands;

public record CreateWorkShiftCommand(string Name, string StartTime, string EndTime, int BreakMinutes) : IRequest<WorkShiftDto>;
